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
        services.AddScoped<IMarketDataService, MarketDataService>();
        services.AddScoped<ICompService, CompService>();

        // Register ZillowApiService with HttpClient
        services.AddHttpClient<ZillowApiService>();
        services.AddScoped<ZillowApiService>();

        // Add more services here as you create them:
        // services.AddScoped<IEvaluationService, EvaluationService>();
        // services.AddScoped<IComparableService, ComparableService>();

        return services;
    }
}