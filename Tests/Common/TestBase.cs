using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Application.Features.Auth.Commands.Register;
using Moq;

namespace Tests.Common;

/// <summary>
/// Shared test fixture that provides an in-memory database and mocked services.
/// Every test gets a fresh database to ensure isolation.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly AppDbContext Context;
    protected readonly Mock<ITokenService> TokenServiceMock;
    protected readonly Mock<IPasswordHasher> PasswordHasherMock;
    protected readonly IOptions<JwtSettingsOption> JwtSettingsOptions;

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);

        TokenServiceMock = new Mock<ITokenService>();
        TokenServiceMock.Setup(x => x.GenerateAccessToken(
                It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
            .Returns("test-access-token");
        TokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        PasswordHasherMock = new Mock<IPasswordHasher>();
        PasswordHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
            .Returns((string pw) => $"hashed_{pw}");
        PasswordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string pw, string hash) => hash == $"hashed_{pw}");

        JwtSettingsOptions = Options.Create(new JwtSettingsOption
        {
            SecretKey                    = "TestSecretKey_MustBe32Characters!",
            Issuer                       = "TestIssuer",
            Audience                     = "TestAudience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays   = 7
        });
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
