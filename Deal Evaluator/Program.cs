using Deal_Evaluator.Data;
using Deal_Evaluator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Deal_Evaluator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        
        // Get connection string
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        
        // Connect DB to the container
        builder.Services.AddDbContext<DealEvaluatorContext>(options =>
            options.UseSqlServer(connectionString));

        
        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<DealEvaluatorContext>()
            .AddDefaultTokenProviders();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}