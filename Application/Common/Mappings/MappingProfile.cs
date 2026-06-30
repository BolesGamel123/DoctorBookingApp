using AutoMapper;
using Domain.Entities;
using Application.Features.Patients.DTOs;
using Application.Features.Admins.DTOs;
using Application.Features.Auth.DTOs;

namespace Application.Common.Mappings;

/// <summary>
/// AutoMapper profile that defines all entity-to-DTO mappings.
/// Auto-discovered by AddAutoMapper() in DependencyInjection.cs.
/// </summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Patient ─────────────────────────────────────────
        CreateMap<patient, PatientResponse>()
            .ForMember(d => d.Id,              opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.FullName,        opt => opt.MapFrom(s => s.full_name))
            .ForMember(d => d.Email,           opt => opt.MapFrom(s => s.email))
            .ForMember(d => d.MobileNumber,    opt => opt.MapFrom(s => s.mobile_number))
            .ForMember(d => d.ProfilePhotoUrl, opt => opt.MapFrom(s => s.profile_photo_url))
            .ForMember(d => d.Birthdate,       opt => opt.MapFrom(s => s.birthdate))
            .ForMember(d => d.IsActive,        opt => opt.MapFrom(s => s.is_active))
            .ForMember(d => d.CreatedAt,       opt => opt.MapFrom(s => s.created_at));

        CreateMap<patient, PatientSummary>()
            .ForMember(d => d.Id,              opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.FullName,        opt => opt.MapFrom(s => s.full_name))
            .ForMember(d => d.Email,           opt => opt.MapFrom(s => s.email))
            .ForMember(d => d.MobileNumber,    opt => opt.MapFrom(s => s.mobile_number))
            .ForMember(d => d.ProfilePhotoUrl, opt => opt.MapFrom(s => s.profile_photo_url))
            .ForMember(d => d.Birthdate,       opt => opt.MapFrom(s => s.birthdate))
            .ForMember(d => d.IsActive,        opt => opt.MapFrom(s => s.is_active))
            .ForMember(d => d.CreatedAt,       opt => opt.MapFrom(s => s.created_at));

        // ── Admin ───────────────────────────────────────────
        CreateMap<admin, AdminResponse>()
            .ForMember(d => d.Id,          opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.FullName,    opt => opt.MapFrom(s => s.full_name))
            .ForMember(d => d.Email,       opt => opt.MapFrom(s => s.email))
            .ForMember(d => d.Role,        opt => opt.MapFrom(s => s.role))
            .ForMember(d => d.IsActive,    opt => opt.MapFrom(s => s.is_active))
            .ForMember(d => d.LastLoginAt, opt => opt.MapFrom(s => s.last_login_at))
            .ForMember(d => d.CreatedAt,   opt => opt.MapFrom(s => s.created_at));
    }
}
