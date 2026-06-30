using Application.Common.Mappings;
using Application.Features.Admins.Commands.CreateAdmin;
using Application.Features.Admins.Commands.DeleteAdmin;
using Application.Features.Admins.Commands.UpdateAdmin;
using Application.Features.Admins.Queries.GetAdminById;
using Application.Features.Admins.Queries.GetAllAdmins;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Tests.Common;

namespace Tests.Features.Admins;

public sealed class AdminHandlerTests : TestBase
{
    private readonly IMapper _mapper;

    public AdminHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    private async Task SeedAdmin(long id = 1, string name = "Super Admin")
    {
        Context.admins.Add(new admin
        {
            id = id, full_name = name, email = $"admin{id}@test.com",
            password = "hashed_pw", role = "admin", is_active = true,
            created_at = DateTime.UtcNow, updated_at = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAdmins_ShouldReturnAll()
    {
        await SeedAdmin(1, "Admin One");
        await SeedAdmin(2, "Admin Two");
        var handler = new GetAllAdminsQueryHandler(new UnitOfWork(Context), _mapper);

        var result = await handler.Handle(new GetAllAdminsQuery(), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAdminById_Existing_ShouldReturn()
    {
        await SeedAdmin(1, "Super Admin");
        var handler = new GetAdminByIdQueryHandler(new UnitOfWork(Context), _mapper);

        var result = await handler.Handle(new GetAdminByIdQuery(1), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.FullName.Should().Be("Super Admin");
    }

    [Fact]
    public async Task GetAdminById_NonExistent_ShouldFail()
    {
        var handler = new GetAdminByIdQueryHandler(new UnitOfWork(Context), _mapper);
        var result = await handler.Handle(new GetAdminByIdQuery(999), CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAdmin_ShouldPersistAndReturn()
    {
        var handler = new CreateAdminCommandHandler(new UnitOfWork(Context), PasswordHasherMock.Object, _mapper);
        var command = new CreateAdminCommand("New Admin", "newadmin@test.com", "Password123", "admin");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.FullName.Should().Be("New Admin");
        result.Data.Email.Should().Be("newadmin@test.com");
        Context.admins.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAdmin_ShouldHashPassword()
    {
        var handler = new CreateAdminCommandHandler(new UnitOfWork(Context), PasswordHasherMock.Object, _mapper);
        await handler.Handle(
            new CreateAdminCommand("Admin", "a@test.com", "MySecret", "admin"), CancellationToken.None);

        Context.admins.First().password.Should().Be("hashed_MySecret");
    }

    [Fact]
    public async Task UpdateAdmin_ShouldModifyFields()
    {
        await SeedAdmin();
        var handler = new UpdateAdminCommandHandler(new UnitOfWork(Context), _mapper);

        var command = new UpdateAdminCommand(1, "Updated Admin", "updated@test.com", "superadmin");
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.FullName.Should().Be("Updated Admin");
        result.Data.Role.Should().Be("superadmin");
    }

    [Fact]
    public async Task UpdateAdmin_NonExistent_ShouldFail()
    {
        var handler = new UpdateAdminCommandHandler(new UnitOfWork(Context), _mapper);
        var result = await handler.Handle(
            new UpdateAdminCommand(999, "X", "x@t.com", "admin"), CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAdmin_ShouldRemove()
    {
        await SeedAdmin();
        var handler = new DeleteAdminCommandHandler(new UnitOfWork(Context));

        var result = await handler.Handle(new DeleteAdminCommand(1), CancellationToken.None);

        result.Success.Should().BeTrue();
        Context.admins.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAdmin_NonExistent_ShouldFail()
    {
        var handler = new DeleteAdminCommandHandler(new UnitOfWork(Context));
        var result = await handler.Handle(new DeleteAdminCommand(999), CancellationToken.None);

        result.Success.Should().BeFalse();
    }
}
