using Application.Common.Mappings;
using Application.Features.Admins.DTOs;
using Application.Features.Auth.DTOs;
using Application.Features.Patients.DTOs;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;

namespace Tests.Common;

/// <summary>
/// Verifies that the AutoMapper profile configuration is valid
/// and all entity-to-DTO mappings produce correct results.
/// </summary>
public sealed class AutoMapperProfileTests
{
    private readonly IMapper _mapper;

    public AutoMapperProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void MappingProfile_Configuration_ShouldBeValid()
    {
        // This validates that ALL mappings are correctly configured
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Should_Map_Patient_To_PatientResponse()
    {
        var entity = new patient
        {
            id = 1, full_name = "John Doe", email = "john@test.com",
            mobile_number = "+20123", is_active = true,
            birthdate = new DateOnly(1990, 5, 15),
            profile_photo_url = "https://photo.url",
            created_at = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var dto = _mapper.Map<PatientResponse>(entity);

        dto.Id.Should().Be(1);
        dto.FullName.Should().Be("John Doe");
        dto.Email.Should().Be("john@test.com");
        dto.MobileNumber.Should().Be("+20123");
        dto.IsActive.Should().BeTrue();
        dto.Birthdate.Should().Be(new DateOnly(1990, 5, 15));
        dto.ProfilePhotoUrl.Should().Be("https://photo.url");
        dto.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Should_Map_Patient_To_PatientSummary()
    {
        var entity = new patient
        {
            id = 42, full_name = "Jane Doe", email = "jane@test.com",
            mobile_number = "+20456", is_active = true,
            created_at = DateTime.UtcNow
        };

        var dto = _mapper.Map<PatientSummary>(entity);

        dto.Id.Should().Be(42);
        dto.FullName.Should().Be("Jane Doe");
        dto.Email.Should().Be("jane@test.com");
    }

    [Fact]
    public void Should_Map_Admin_To_AdminResponse()
    {
        var entity = new admin
        {
            id = 1, full_name = "Super Admin", email = "admin@test.com",
            role = "superadmin", is_active = true,
            last_login_at = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc),
            created_at = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var dto = _mapper.Map<AdminResponse>(entity);

        dto.Id.Should().Be(1);
        dto.FullName.Should().Be("Super Admin");
        dto.Email.Should().Be("admin@test.com");
        dto.Role.Should().Be("superadmin");
        dto.IsActive.Should().BeTrue();
        dto.LastLoginAt.Should().Be(new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Should_Handle_NullValues_Gracefully()
    {
        var entity = new patient
        {
            id = 1, full_name = "Test", email = "t@t.com",
            mobile_number = "+20", is_active = true,
            profile_photo_url = null, birthdate = null,
            created_at = DateTime.UtcNow
        };

        var dto = _mapper.Map<PatientResponse>(entity);

        dto.ProfilePhotoUrl.Should().BeNull();
        dto.Birthdate.Should().BeNull();
    }
}
