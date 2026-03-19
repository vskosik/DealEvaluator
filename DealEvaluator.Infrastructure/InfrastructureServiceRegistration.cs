using System;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Infrastructure.Data;
using DealEvaluator.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DealEvaluator.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DealEvaluatorContext");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DealEvaluatorContext' is not configured. " +
                "Set ConnectionStrings__DealEvaluatorContext in environment variables or app configuration.");
        }

        services.AddDbContext<DealEvaluatorContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)));

        services.AddScoped(typeof(IRepository<>), typeof(DbRepository<>));
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<IMarketDataRepository, MarketDataRepository>();
        services.AddScoped<IRehabCostTemplateRepository, RehabCostTemplateRepository>();
        services.AddScoped<IDealSettingsRepository, DealSettingsRepository>();
        services.AddScoped<ILenderRepository, LenderRepository>();

        return services;
    }
}
