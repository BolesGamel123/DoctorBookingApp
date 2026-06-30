using Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Registers all Application layer services into the DI container.
/// Called from Program.cs: builder.Services.AddApplication();
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR — auto-discovers all IRequestHandler implementations
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation — auto-discovers all AbstractValidator<T> implementations
        services.AddValidatorsFromAssembly(assembly);

        // Validation pipeline — runs validators before every handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // AutoMapper — auto-discovers all Profile implementations
        services.AddAutoMapper(assembly);

        return services;
    }
}
