using Application.Common.Mappings;
using Application.Features.Patients.Commands.DeletePatient;
using Application.Features.Patients.Commands.UpdatePatient;
using Application.Features.Patients.Queries.GetAllPatients;
using Application.Features.Patients.Queries.GetCurrentPatient;
using Application.Features.Patients.Queries.GetPatientById;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Tests.Common;

namespace Tests.Features.Patients;

public sealed class PatientHandlerTests : TestBase
{
    private readonly IMapper _mapper;

    public PatientHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    private async Task<patient> SeedPatient(long id = 1, string name = "John Doe")
    {
        var p = new patient
        {
            id = id, full_name = name, email = $"{name.ToLower().Replace(" ", "")}@test.com",
            mobile_number = $"+20100000000{id}", password = "hashed_pw",
            is_active = true, created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
        };
        Context.patients.Add(p);
        await Context.SaveChangesAsync();
        return p;
    }

    // ── GetCurrentPatient ────────────────────────────────────
    [Fact]
    public async Task GetCurrentPatient_ExistingPatient_ShouldReturnProfile()
    {
        await SeedPatient();
        var handler = new GetCurrentPatientQueryHandler(new UnitOfWork(Context), _mapper);

        var result = await handler.Handle(new GetCurrentPatientQuery(1), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetCurrentPatient_NonExistent_ShouldFail()
    {
        var handler = new GetCurrentPatientQueryHandler(new UnitOfWork(Context), _mapper);
        var result = await handler.Handle(new GetCurrentPatientQuery(999), CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetCurrentPatient_SoftDeleted_ShouldFail()
    {
        var p = await SeedPatient();
        p.deleted_at = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        var handler = new GetCurrentPatientQueryHandler(new UnitOfWork(Context), _mapper);
        var result = await handler.Handle(new GetCurrentPatientQuery(1), CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    // ── GetPatientById ───────────────────────────────────────
    [Fact]
    public async Task GetPatientById_ShouldReturnPatient()
    {
        await SeedPatient(5, "Jane Doe");
        var handler = new GetPatientByIdQueryHandler(new UnitOfWork(Context), _mapper);

        var result = await handler.Handle(new GetPatientByIdQuery(5), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.FullName.Should().Be("Jane Doe");
    }

    // ── GetAllPatients ───────────────────────────────────────
    [Fact]
    public async Task GetAllPatients_ShouldReturnPaginatedResults()
    {
        for (int i = 1; i <= 25; i++)
            await SeedPatient(i, $"Patient {i}");

        var handler = new GetAllPatientsQueryHandler(new UnitOfWork(Context), _mapper);

        var result = await handler.Handle(new GetAllPatientsQuery(1, 10), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(10);
        result.Data.TotalCount.Should().Be(25);
        result.Data.TotalPages.Should().Be(3);
        result.Data.HasNextPage.Should().BeTrue();
        result.Data.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllPatients_Page2_ShouldReturnCorrectSlice()
    {
        for (int i = 1; i <= 25; i++)
            await SeedPatient(i, $"Patient {i}");

        var handler = new GetAllPatientsQueryHandler(new UnitOfWork(Context), _mapper);
        var result = await handler.Handle(new GetAllPatientsQuery(2, 10), CancellationToken.None);

        result.Data!.Items.Should().HaveCount(10);
        result.Data.HasPreviousPage.Should().BeTrue();
        result.Data.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllPatients_ExcludesSoftDeleted()
    {
        await SeedPatient(1, "Active");
        await SeedPatient(2, "Deleted");
        Context.patients.First(x => x.id == 2).deleted_at = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        var handler = new GetAllPatientsQueryHandler(new UnitOfWork(Context), _mapper);
        var result = await handler.Handle(new GetAllPatientsQuery(), CancellationToken.None);

        result.Data!.TotalCount.Should().Be(1);
    }

    // ── UpdatePatient ────────────────────────────────────────
    [Fact]
    public async Task UpdatePatient_ShouldUpdateOnlyProvidedFields()
    {
        await SeedPatient();
        var handler = new UpdatePatientCommandHandler(new UnitOfWork(Context), _mapper);

        var command = new UpdatePatientCommand(1, "Updated Name", null, null, null, null, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.FullName.Should().Be("Updated Name");
        result.Data.Email.Should().Be("johndoe@test.com");
    }

    [Fact]
    public async Task UpdatePatient_NonExistent_ShouldFail()
    {
        var handler = new UpdatePatientCommandHandler(new UnitOfWork(Context), _mapper);
        var command = new UpdatePatientCommand(999, "Name", null, null, null, null, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    // ── DeletePatient ────────────────────────────────────────
    [Fact]
    public async Task DeletePatient_ShouldSoftDelete()
    {
        await SeedPatient();
        var handler = new DeletePatientCommandHandler(new UnitOfWork(Context));

        var result = await handler.Handle(new DeletePatientCommand(1), CancellationToken.None);

        result.Success.Should().BeTrue();
        var patient = Context.patients.First();
        patient.deleted_at.Should().NotBeNull();
        patient.is_active.Should().BeFalse();
        patient.refresh_token.Should().BeNull();
    }

    [Fact]
    public async Task DeletePatient_AlreadyDeleted_ShouldFail()
    {
        var p = await SeedPatient();
        p.deleted_at = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        var handler = new DeletePatientCommandHandler(new UnitOfWork(Context));
        var result = await handler.Handle(new DeletePatientCommand(1), CancellationToken.None);

        result.Success.Should().BeFalse();
    }
}
