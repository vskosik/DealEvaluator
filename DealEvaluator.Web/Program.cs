using DealEvaluator.Application;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Infrastructure;
using DealEvaluator.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

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
        
        builder.Services.AddAuthorization();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapIdentityApi<User>();
        
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}