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

        services.AddDbContext<DealEvaluatorContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(DbRepository<>));
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<IMarketDataRepository, MarketDataRepository>();

        return services;
    }
}