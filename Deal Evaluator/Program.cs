using Deal_Evaluator.API;
using Deal_Evaluator.Data;
using Deal_Evaluator.Extensions;
using Deal_Evaluator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Deal_Evaluator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add controllers and views
        builder.Services.AddControllersWithViews();
        
        // Get connection string
        var connectionString = builder.Configuration.GetConnectionString("DealEvaluatorContext");
        
        // Config DbContext
        builder.Services.AddDbContext<DealEvaluatorContext>(options =>
            options.UseSqlServer(connectionString));

        // Config Auth Services
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme)
            .AddBearerToken(IdentityConstants.BearerScheme);

        // Config ASP.NET Identity
        builder.Services.AddIdentityCore<User>()
            .AddEntityFrameworkStores<DealEvaluatorContext>()
            .AddApiEndpoints();
        
        // Swagger Services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddHttpClient<ZillowApiService>();
        builder.Services.AddScoped<ZillowApiService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        
        // Start Swagger
        app.UseSwagger();
        app.UseSwaggerUI();
        
        // Apply DB Migration
        // app.ApplyMigrations();
        
        // Seeding DB with test data
        /*using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DealEvaluatorContext>();
            SeedDatabase(context);
        }*/

        app.MapIdentityApi<User>();
        
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }

    public static void SeedDatabase(DealEvaluatorContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            var userList = new List<User>
            {
                new User
                {
                    UserName = "john.doe@example.com",
                    Email = "john.doe@example.com",
                    CompanyName = "Doe Real Estate",
                    NormalizedUserName = "JOHN.DOE@EXAMPLE.COM",
                    NormalizedEmail = "JOHN.DOE@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new User
                {
                    UserName = "jane.smith@example.com",
                    Email = "jane.smith@example.com",
                    CompanyName = "Smith Investment Group",
                    NormalizedUserName = "JANE.SMITH@EXAMPLE.COM",
                    NormalizedEmail = "JANE.SMITH@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new User
                {
                    UserName = "michael.johnson@example.com",
                    Email = "michael.johnson@example.com",
                    CompanyName = "Johnson Home Solutions",
                    NormalizedUserName = "MICHAEL.JOHNSON@EXAMPLE.COM",
                    NormalizedEmail = "MICHAEL.JOHNSON@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            };
            
            
            var pwHasher = new PasswordHasher<User>();
            
            userList[0].PasswordHash = pwHasher.HashPassword(userList[0], "Admin123");
            userList[1].PasswordHash = pwHasher.HashPassword(userList[1], "User1234");
            userList[2].PasswordHash = pwHasher.HashPassword(userList[2], "Guest123");
            
            context.Users.AddRange(userList);
        }

        if (!context.Properties.Any())
        {
            var propertyList = new List<Property>
            {
                new Property
                {
                    UserId = "7a74698d-3cff-49f7-85ae-987f4828d907",
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
                    UserId = "956502c6-58b8-40db-bd54-4f18f1136ab4",
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
                    UserId = "c4f0185d-13ed-4cbf-aa12-0f31cddf5f98",
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
                    PropertyId = 1, // Make sure this property exists in the DB
                    Source = "Zillow",
                    DataJson = "{ \"price\": 250000, \"sqft\": 1500, \"bedrooms\": 3, \"bathrooms\": 2 }",
                    LastUpdated = DateTime.UtcNow
                },
                new MarketData
                {
                    PropertyId = 1,
                    Source = "Redfin",
                    DataJson = "{ \"price\": 255000, \"sqft\": 1550, \"bedrooms\": 3, \"bathrooms\": 2 }",
                    LastUpdated = DateTime.UtcNow
                },
                new MarketData
                {
                    PropertyId = 2, // Different property
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
                    PropertyId = 1, // Ensure this property exists in the DB
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
                    PropertyId = 1,
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
                    PropertyId = 2, // Different property
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
                    PropertyId = 1, // Ensure this property exists in the DB
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
                    PropertyId = 1,
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
                    PropertyId = 2, // Different property
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
                    PropertyId = 1, // Ensure this property exists
                    UserId = "7a74698d-3cff-49f7-85ae-987f4828d907", // Ensure this user exists
                    Endpoint = "/api/properties/1",
                    RequestData = "{ \"request\": \"Get property details\" }",
                    ResponseData = "{ \"response\": \"Success\", \"data\": { \"price\": 320000 } }",
                    Success = true,
                    ErrorMessage = null,
                    CreatedAt = DateTime.UtcNow
                },
                new ApiLog
                {
                    PropertyId = 2,
                    UserId = "956502c6-58b8-40db-bd54-4f18f1136ab4",
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
                    UserId = "c4f0185d-13ed-4cbf-aa12-0f31cddf5f98",
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

        context.SaveChanges();
    }
}