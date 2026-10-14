using System.IdentityModel.Tokens.Jwt;
using Application.Common.Interfaces;
using Application.Features.Auth;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Tests;

public sealed class AuthTests
{
    private static readonly JwtSettingsOption Settings = new()
    {
        SecretKey = "test-only-signing-key-32-bytes-minimum-do-not-deploy",
        Issuer = "DoctorBookingApp", Audience = "DoctorBookingApp"
    };

    [Theory]
    [InlineData(AccountRoles.Patient)]
    [InlineData(AccountRoles.Doctor)]
    [InlineData(AccountRoles.Admin)]
    public async Task LoginRefreshAndLogoutWorkForEachRole(string role)
    {
        await using var store = await TestStore.CreateAsync();
        await using var db = store.Open();
        var service = Sessions(db);
        var handler = new LoginCommandHandler(db, new PasswordHasher(), service);
        var login = await handler.Handle(new LoginCommand("shared@example.test", "Password123!", role), default);
        Assert.True(login.Success);
        var response = login.Data!;
        Assert.Equal(role, response.Role);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);
        var sid = Guid.Parse(jwt.Claims.Single(x => x.Type == "sid").Value);
        Assert.True(await service.IsValidAsync(role, 1, sid, default));
        var stored = await db.AuthenticationSessions.SingleAsync();
        Assert.NotEqual(response.RefreshToken, stored.RefreshTokenHash);
        Assert.Equal(64, stored.RefreshTokenHash.Length);
        var next = await service.RefreshAsync(response.RefreshToken, default);
        Assert.NotNull(next);
        Assert.NotEqual(response.RefreshToken, next.RefreshToken);
        Assert.Null(await service.RefreshAsync(response.RefreshToken, default));
        Assert.True(await service.RevokeAsync(role, 1, sid, default));
        Assert.False(await service.IsValidAsync(role, 1, sid, default));
        Assert.Null(await service.RefreshAsync(next.RefreshToken, default));
    }

    [Fact]
    public async Task OverlappingIdsCannotRevokeAnotherRole()
    {
        await using var store = await TestStore.CreateAsync();
        await using var db = store.Open();
        var service = Sessions(db);
        var patient = await service.IssueAsync(AuthAccount.From(await db.patients.SingleAsync()), default);
        var doctor = await service.IssueAsync(AuthAccount.From(await db.doctors.SingleAsync()), default);
        var doctorSid = Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(doctor.AccessToken)
            .Claims.Single(x => x.Type == "sid").Value);
        Assert.False(await service.RevokeAsync(AccountRoles.Patient, 1, doctorSid, default));
        Assert.True(await service.RevokeAsync(AccountRoles.Doctor, 1, doctorSid, default));
        Assert.NotNull(await service.RefreshAsync(patient.RefreshToken, default));
    }

    [Fact]
    public async Task BannedInactiveDeletedAndExpiredAccountsCannotRefresh()
    {
        await using var store = await TestStore.CreateAsync();
        await using var db = store.Open();
        var service = Sessions(db);
        var account = await db.patients.SingleAsync();
        var response = await service.IssueAsync(AuthAccount.From(account), default);
        account.is_banned = true;
        await db.SaveChangesAsync();
        Assert.Null(await service.RefreshAsync(response.RefreshToken, default));
        account.is_banned = false;
        account.is_active = false;
        await db.SaveChangesAsync();
        Assert.Null(await service.RefreshAsync(response.RefreshToken, default));
        account.is_active = true;
        account.deleted_at = DateTime.UtcNow;
        await db.SaveChangesAsync();
        Assert.Null(await service.RefreshAsync(response.RefreshToken, default));
        account.deleted_at = null;
        (await db.AuthenticationSessions.SingleAsync()).ExpiresAt = DateTime.UtcNow.AddSeconds(-1);
        await db.SaveChangesAsync();
        Assert.Null(await service.RefreshAsync(response.RefreshToken, default));
        Assert.Null(await service.RefreshAsync(null!, default));
    }

    [Fact]
    public async Task AmbiguousLoginAndExistingSecurityRequirementsAreRejected()
    {
        await using var store = await TestStore.CreateAsync();
        await using var db = store.Open();
        var handler = new LoginCommandHandler(db, new PasswordHasher(), Sessions(db));
        Assert.False((await handler.Handle(new LoginCommand("shared@example.test", "Password123!"), default)).Success);
        Assert.False((await handler.Handle(new LoginCommand("shared@example.test", "wrong", "Patient"), default)).Success);
        (await db.admins.SingleAsync()).two_factor_enabled = true;
        (await db.doctors.SingleAsync()).must_change_password = true;
        await db.SaveChangesAsync();
        Assert.False((await handler.Handle(new LoginCommand("shared@example.test", "Password123!", "Admin"), default)).Success);
        Assert.False((await handler.Handle(new LoginCommand("shared@example.test", "Password123!", "Doctor"), default)).Success);
    }

    [Fact]
    public async Task NewLoginInvalidatesPreviousSession()
    {
        await using var store = await TestStore.CreateAsync();
        await using var db = store.Open();
        var service = Sessions(db);
        var account = AuthAccount.From(await db.patients.SingleAsync());
        var first = await service.IssueAsync(account, default);
        var second = await service.IssueAsync(account, default);
        var firstSid = Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(first.AccessToken)
            .Claims.Single(x => x.Type == "sid").Value);
        Assert.False(await service.IsValidAsync("Patient", 1, firstSid, default));
        Assert.Null(await service.RefreshAsync(first.RefreshToken, default));
        Assert.NotNull(await service.RefreshAsync(second.RefreshToken, default));
    }

    [Fact]
    public async Task TwoReadersCannotBothConsumeTheSameRefreshToken()
    {
        await using var store = await TestStore.CreateAsync();
        await using var firstDb = store.Open();
        await using var secondDb = store.Open();
        var firstService = Sessions(firstDb);
        var response = await firstService.IssueAsync(AuthAccount.From(await firstDb.patients.SingleAsync()), default);
        // Load the original hash in both contexts, then commit the competing update first.
        // This exercises actual relational UPDATE concurrency, without timing-dependent thread scheduling.
        await secondDb.AuthenticationSessions.SingleAsync();
        Assert.NotNull(await firstService.RefreshAsync(response.RefreshToken, default));
        // Force the second reader to use its already-read version, as two simultaneous requests would.
        var stale = await secondDb.AuthenticationSessions.SingleAsync();
        stale.RefreshTokenHash = new string('A', 64);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondDb.SaveChangesAsync());
        await using var fresh = store.Open();
        Assert.Null(await Sessions(fresh).RefreshAsync(response.RefreshToken, default));
    }

    [Fact]
    public void PasswordValidationBoundsBcryptInputBytes()
    {
        var validator = new RegisterCommandValidator();
        Assert.False(validator.Validate(new RegisterCommand("Name", "a@example.test", "123",
            new string('ع', 40), new string('ع', 40))).IsValid);
        Assert.True(validator.Validate(new RegisterCommand("Name", "a@example.test", "123",
            "Password123!", "Password123!")).IsValid);
        Assert.False(new LoginCommandValidator().Validate(new LoginCommand("a@example.test", "pass", "SuperAdmin")).IsValid);
    }

    private static AuthenticationSessionService Sessions(AppDbContext db)
        => new(db, new TokenService(Options.Create(Settings)), Options.Create(Settings));

    private sealed class TestStore : IAsyncDisposable
    {
        private readonly SqliteConnection _keeper;
        public string ConnectionString { get; }
        private TestStore()
        {
            ConnectionString = $"Data Source=auth-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
            _keeper = new SqliteConnection(ConnectionString);
        }
        public SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            connection.CreateFunction("sysutcdatetime", () => DateTime.UtcNow.ToString("O"));
            return connection;
        }
        public AppDbContext Open() => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(OpenConnection()).Options);
        public static async Task<TestStore> CreateAsync()
        {
            var store = new TestStore();
            await store._keeper.OpenAsync();
            // SQL Server's timestamp defaults are not available in SQLite.
            store._keeper.CreateFunction("sysutcdatetime", () => DateTime.UtcNow.ToString("O"));
            await using var db = store.Open();
            await db.Database.EnsureCreatedAsync();
            var password = new PasswordHasher().Hash("Password123!");
            db.patients.Add(new patient
            {
                id = 1, full_name = "Patient", email = "shared@example.test", mobile_number = "1",
                password = password, is_active = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
            });
            db.admins.Add(new admin
            {
                id = 1, full_name = "Admin", email = "shared@example.test", password = password, role = "admin",
                is_active = true, two_factor_enabled = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
            });
            db.specialties.Add(new specialty { id = 1, name = "Test" });
            db.doctors.Add(new doctor
            {
                id = 1, admin_id = 1, specialty_id = 1, full_name = "Doctor", email = "shared@example.test",
                mobile_number = "2", password = password, license_number = "test", currency = "USD",
                is_active = true, must_change_password = false, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
            });
            // Explicit false must override database true defaults on existing security fields.
            await db.SaveChangesAsync();
            // EF treats false as the sentinel for default-true fields; seed the intended states explicitly.
            await db.Database.ExecuteSqlRawAsync("UPDATE admins SET two_factor_enabled = 0");
            await db.Database.ExecuteSqlRawAsync("UPDATE doctors SET must_change_password = 0");
            return store;
        }
        public ValueTask DisposeAsync() => _keeper.DisposeAsync();
    }
}
