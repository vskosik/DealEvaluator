using DealEvaluator.Application.Interfaces;
using DealEvaluator.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DealEvaluator.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(ServiceRegistration).Assembly);

        // Register Application Services
        services.AddScoped<IPropertyService, PropertyService>();
        // Add more services here as you create them:
        // services.AddScoped<IEvaluationService, EvaluationService>();
        // services.AddScoped<IComparableService, ComparableService>();

        return services;
    }
}