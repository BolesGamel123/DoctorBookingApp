using System.Text;
using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Register;
using Application.Profiles;
using Infrastructure.Persistence;
using Infrastructure.Repsoitory;
using Infrastructure.Services;
using Infrastructure.UnitOfWorks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

/// <summary>
/// Registers all Infrastructure layer services into the DI container.
/// Called from Program.cs: builder.Services.AddInfrastructure(builder.Configuration);
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── Database ─────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        // ── JWT Settings ─────────────────────────────────────
        services.Configure<JwtSettingsOption>(
            configuration.GetSection(JwtSettingsOption.SectionName));

        var jwtSettings = configuration
            .GetSection(JwtSettingsOption.SectionName)
            .Get<JwtSettingsOption>()!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;  // keep 'sub' as 'sub', not remapped to NameIdentifier
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = jwtSettings.Issuer,
                    ValidAudience            = jwtSettings.Audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                   Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };
            });

        services.AddAuthorization();

        // ── Services ─────────────────────────────────────────
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IDoctorFavoriteService, Doctor_FavouriteService>();
        services.AddScoped<ISearchHistoryService, SearchHistoryService>();
        services.AddScoped<ISystemLogRepository, SystemLogRepository>();
        services.AddScoped<IUnitOfWorks, Unit_Of_Work>();
        services.AddAutoMapper(typeof(DoctorFavoriteProfile).Assembly);

        // ── Stripe ───────────────────────────────────────────
        services.Configure<StripeSettings>(
            configuration.GetSection(StripeSettings.SectionName));
        services.AddScoped<IStripeService, StripeService>();

        // StripePublicSettings gives Application layer access to the publishable key only
        services.Configure<Application.Common.Models.StripePublicSettings>(
            configuration.GetSection(Application.Common.Models.StripePublicSettings.SectionName));

        return services;
    }
}
