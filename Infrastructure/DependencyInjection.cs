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
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;

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
        services.AddOptions<JwtSettingsOption>()
            .Bind(configuration.GetSection(JwtSettingsOption.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.SecretKey)
                && Encoding.UTF8.GetByteCount(x.SecretKey) >= 32,
                "Set JwtSettings:SecretKey using a secret store or environment variable (at least 32 UTF-8 bytes).")
            .Validate(x => !string.IsNullOrWhiteSpace(x.Issuer) && !string.IsNullOrWhiteSpace(x.Audience),
                "JWT issuer and audience are required.")
            .Validate(x => x.AccessTokenExpirationMinutes is > 0 and <= 60
                && x.RefreshTokenExpirationDays is > 0 and <= 30, "JWT lifetimes are outside supported bounds.")
            .ValidateOnStart();

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
                    ValidAlgorithms          = [SecurityAlgorithms.HmacSha256],
                    ClockSkew                = TimeSpan.FromSeconds(30),
                    ValidIssuer              = jwtSettings.Issuer,
                    ValidAudience            = jwtSettings.Audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                   Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal!;
                        var roles = principal.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
                        if (roles.Length != 1 || !AccountRoles.IsValid(roles[0])
                            || !long.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId)
                            || !Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sid), out var sessionId))
                        {
                            context.Fail("Invalid identity claims.");
                            return;
                        }
                        var sessions = context.HttpContext.RequestServices
                            .GetRequiredService<IAuthenticationSessionService>();
                        if (!await sessions.IsValidAsync(roles[0], userId, sessionId,
                                context.HttpContext.RequestAborted))
                            context.Fail("Session is unavailable.");
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken)
                            && path.StartsWithSegments(new PathString("/hubs/chat")))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        // ── Services ─────────────────────────────────────────
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IDoctorFavoriteService, Doctor_FavouriteService>();
        services.AddScoped<ISearchHistoryService, SearchHistoryService>();
        services.AddScoped<ISystemLogRepository, SystemLogRepository>();
        services.AddScoped<ISystemLogService, SystemLogService>();
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
