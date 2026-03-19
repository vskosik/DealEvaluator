using DealEvaluator.Application;
using DealEvaluator.Application.Authorization;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Infrastructure;
using DealEvaluator.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add controllers and views
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // Register Application Services
        builder.Services.AddApplicationServices();

        // Register Infrastructure Services
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // Config ASP.NET Identity
        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<DealEvaluatorContext>()
            .AddApiEndpoints();

        // Configure authorization policies
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("PropertyOwner", policy =>
                policy.Requirements.Add(new PropertyOwnerRequirement()));
        });

        // Register authorization handlers
        builder.Services.AddScoped<IAuthorizationHandler, PropertyOwnershipHandler>();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Required when running behind cloud load balancers/reverse proxies (e.g., Azure App Service).
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
        
        var app = builder.Build();

        var applyMigrationsOnStartup = app.Configuration.GetValue(
            "Database:ApplyMigrationsOnStartup",
            app.Environment.IsDevelopment());

        if (applyMigrationsOnStartup)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DealEvaluatorContext>();
            await dbContext.Database.MigrateAsync();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseForwardedHeaders();

        var enableSwagger = app.Configuration.GetValue("Swagger:Enabled", app.Environment.IsDevelopment());
        if (enableSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapIdentityApi<User>();

        // Only use HTTPS redirection in production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
