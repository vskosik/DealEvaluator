using Deal_Evaluator.API;
using Deal_Evaluator.Data;
using Deal_Evaluator.Extensions;
using Deal_Evaluator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Deal_Evaluator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add controllers and views
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        
        // Config DbContext
        var connectionString = builder.Configuration.GetConnectionString("DealEvaluatorContext");
        
        builder.Services.AddDbContext<DealEvaluatorContext>(options =>
            options.UseSqlServer(connectionString));

        // Config ASP.NET Identity
        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<DealEvaluatorContext>()
            .AddApiEndpoints();
        
        builder.Services.AddAuthorization();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // DI
        builder.Services.AddHttpClient<ZillowApiService>();
        builder.Services.AddScoped<ZillowApiService>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        
        app.UseSwagger();
        app.UseSwaggerUI();
        
        // Apply DB Migration
        // app.ApplyMigrations();
        
        // Seeding DB with test data
        /*using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DealEvaluatorContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            await SeedDatabase(context, userManager, roleManager);
        }*/

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

    private static async Task SeedDatabase(DealEvaluatorContext context, UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        if (!roleManager.Roles.Any())
        {
            var roles = new [] { "Admin", "User" };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!context.Users.Any())
        {
            var userAdmin = new User { UserName = "admin@admin.com", Email = "Admin@admin.com", CompanyName = "AdminCompany" };
            var userUser = new User { UserName = "user@user.com", Email = "user@user.com", CompanyName = "UserCompany"};

            await userManager.CreateAsync(userAdmin, "Admin123!");
            await userManager.AddToRoleAsync(userAdmin, "Admin");
            
            await userManager.CreateAsync(userUser, "User123!");
            await userManager.AddToRoleAsync(userUser, "User");
        }

        if (!context.Properties.Any())
        {
            var propertyList = new List<Property>
            {
                new Property
                {
                    UserId = "5b377fd9-e9ed-4608-9bd2-b6ec6058f077",
                    Address = "123 Main St",
                    City = "Los Angeles",
                    State = "CA",
                    ZipCode = "90001",
                    Price = 320000,
                    Sqft = 1500,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    LotSizeSqft = 5000,
                    YearBuilt = 1990,
                    PropertyType = PropertyTypes.SingleFamily,
                    PropertyConditions = PropertyConditions.MinorRepairs,
                    CreatedAt = DateTime.UtcNow
                },
                new Property
                {
                    UserId = "5b377fd9-e9ed-4608-9bd2-b6ec6058f077",
                    Address = "456 Oak St",
                    City = "Los Angeles",
                    State = "CA",
                    ZipCode = "90002",
                    Price = 310000,
                    Sqft = 1400,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    LotSizeSqft = 4500,
                    YearBuilt = 1985,
                    PropertyType = PropertyTypes.MultiFamily,
                    PropertyConditions = PropertyConditions.Outdated,
                    CreatedAt = DateTime.UtcNow
                },
                new Property
                {
                    UserId = "5b377fd9-e9ed-4608-9bd2-b6ec6058f077",
                    Address = "789 Pine St",
                    City = "San Diego",
                    State = "CA",
                    ZipCode = "92101",
                    Price = 400000,
                    Sqft = 1700,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    LotSizeSqft = 6000,
                    YearBuilt = 2000,
                    PropertyType = PropertyTypes.Condo,
                    PropertyConditions = PropertyConditions.Horrible,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            context.Properties.AddRange(propertyList);
        }
        
        if (!context.MarketData.Any())
        {
            var marketDataList = new List<MarketData>
            {
                new MarketData
                {
                    PropertyId = 1003, // Make sure this property exists in the DB
                    Source = "Zillow",
                    DataJson = "{ \"price\": 250000, \"sqft\": 1500, \"bedrooms\": 3, \"bathrooms\": 2 }",
                    LastUpdated = DateTime.UtcNow
                },
                new MarketData
                {
                    PropertyId = 1003,
                    Source = "Redfin",
                    DataJson = "{ \"price\": 255000, \"sqft\": 1550, \"bedrooms\": 3, \"bathrooms\": 2 }",
                    LastUpdated = DateTime.UtcNow
                },
                new MarketData
                {
                    PropertyId = 1004, // Different property
                    Source = "Realtor.com",
                    DataJson = "{ \"price\": 260000, \"sqft\": 1600, \"bedrooms\": 4, \"bathrooms\": 2.5 }",
                    LastUpdated = DateTime.UtcNow
                }
            };
            
            context.MarketData.AddRange(marketDataList);
        }
        
        if (!context.Evaluations.Any())
        {
            var evaluationList = new List<Evaluation>
            {
                new Evaluation
                {
                    PropertyId = 1003, // Ensure this property exists in the DB
                    Arv = 300000,
                    RepairCost = 50000,
                    PurchasePrice = 220000,
                    RentalIncome = 1800,
                    CapRate = 8,
                    CashOnCash = 12,
                    CreatedAt = DateTime.UtcNow
                },
                new Evaluation
                {
                    PropertyId = 1003,
                    Arv = 310000,
                    RepairCost = 45000,
                    PurchasePrice = 225000,
                    RentalIncome = 1850,
                    CapRate = 8,
                    CashOnCash = 13,
                    CreatedAt = DateTime.UtcNow
                },
                new Evaluation
                {
                    PropertyId = 1005, // Different property
                    Arv = 400000,
                    RepairCost = 60000,
                    PurchasePrice = 300000,
                    RentalIncome = 2500,
                    CapRate = 9,
                    CashOnCash = 14,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            context.Evaluations.AddRange(evaluationList);
        }

        if (!context.Comparables.Any())
        {
            var comparableList = new List<Comparable>
            {
                new Comparable
                {
                    PropertyId = 1004, // Ensure this property exists in the DB
                    Address = "123 Main St",
                    City = "Los Angeles",
                    State = "CA",
                    ZipCode = "90001",
                    Price = 320000,
                    Sqft = 1500,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    LotSizeSqft = 5000,
                    YearBuilt = 1990,
                    SaleDate = DateTime.UtcNow.AddMonths(-2), // Sold 2 months ago
                    ListingStatus = ListingStatuses.Sold,
                    Source = "MLS"
                },
                new Comparable
                {
                    PropertyId = 1005,
                    Address = "456 Oak St",
                    City = "Los Angeles",
                    State = "CA",
                    ZipCode = "90002",
                    Price = 310000,
                    Sqft = 1400,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    LotSizeSqft = 4500,
                    YearBuilt = 1985,
                    SaleDate = DateTime.UtcNow.AddMonths(-3), // Sold 3 months ago
                    ListingStatus = ListingStatuses.Sold,
                    Source = "Zillow"
                },
                new Comparable
                {
                    PropertyId = 1005, // Different property
                    Address = "789 Pine St",
                    City = "San Diego",
                    State = "CA",
                    ZipCode = "92101",
                    Price = 400000,
                    Sqft = 1700,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    LotSizeSqft = 6000,
                    YearBuilt = 2000,
                    SaleDate = DateTime.UtcNow.AddMonths(-1), // Sold 1 month ago
                    ListingStatus = ListingStatuses.Sold,
                    Source = "Redfin"
                }
            };
            
            context.Comparables.AddRange(comparableList);
        }

        if (!context.ApiLogs.Any())
        {
            var apiLogList = new List<ApiLog>
            {
                new ApiLog
                {
                    PropertyId = 1003, // Ensure this property exists
                    UserId = "5b377fd9-e9ed-4608-9bd2-b6ec6058f077", // Ensure this user exists
                    Endpoint = "/api/properties/1",
                    RequestData = "{ \"request\": \"Get property details\" }",
                    ResponseData = "{ \"response\": \"Success\", \"data\": { \"price\": 320000 } }",
                    Success = true,
                    ErrorMessage = null,
                    CreatedAt = DateTime.UtcNow
                },
                new ApiLog
                {
                    PropertyId = 1003,
                    UserId = "5b377fd9-e9ed-4608-9bd2-b6ec6058f077",
                    Endpoint = "/api/properties/2",
                    RequestData = "{ \"request\": \"Get property details\" }",
                    ResponseData = "{ \"response\": \"Success\", \"data\": { \"price\": 310000 } }",
                    Success = true,
                    ErrorMessage = null,
                    CreatedAt = DateTime.UtcNow
                },
                new ApiLog
                {
                    PropertyId = null, // Simulating a request not tied to a property
                    UserId = "de07f6af-d0bb-45e5-962b-93a08d62d928",
                    Endpoint = "/api/auth/login",
                    RequestData = "{ \"username\": \"testuser\", \"password\": \"****\" }",
                    ResponseData = "{ \"response\": \"Failure\" }",
                    Success = false,
                    ErrorMessage = "Invalid credentials",
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            context.ApiLogs.AddRange(apiLogList);
        }

        await context.SaveChangesAsync();
    }
}