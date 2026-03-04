using AutoMapper;
using DealEvaluator.Application.DTOs.Evaluation;
using DealEvaluator.Application.DTOs.Rehab;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Application.Mappings;
using DealEvaluator.Application.Services;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Domain.Enums;
using DealEvaluator.Infrastructure.Data;
using DealEvaluator.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Tests;

public class PropertyServiceEvaluationTests
{
    [Fact]
    public async Task CreateEvaluationAsync_Throws_WhenNoComparableIdsProvided()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var property = await SeedPropertyAsync(context);

        var dto = new CreateEvaluationDto
        {
            PropertyId = property.Id,
            ComparableIds = new List<int>(),
            RehabLineItems = BuildRehabLineItems()
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateEvaluationAsync(dto, "user-1"));
    }

    [Fact]
    public async Task CreateEvaluationAsync_Throws_WhenSelectedLenderDoesNotExist()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var property = await SeedPropertyAsync(context);
        var compIds = await SeedComparablesAsync(context, property.Id);
        await SeedSettingsAsync(context, "user-1");

        var dto = new CreateEvaluationDto
        {
            PropertyId = property.Id,
            ComparableIds = compIds,
            RehabLineItems = BuildRehabLineItems(),
            LenderId = 999,
            UseDefaultLoanRate = false
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateEvaluationAsync(dto, "user-1"));
    }

    [Fact]
    public async Task CreateEvaluationAsync_UsesSelectedLender_WhenProvided()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var property = await SeedPropertyAsync(context);
        var compIds = await SeedComparablesAsync(context, property.Id);
        await SeedSettingsAsync(context, "user-1", defaultLoanRate: 0.08);

        var lender = new Lender
        {
            UserId = "user-1",
            Name = "Test Lender",
            AnnualRate = 0.15,
            OriginationFee = 0.02,
            LoanServiceFee = 0.01
        };
        context.Lenders.Add(lender);
        await context.SaveChangesAsync();

        var dto = new CreateEvaluationDto
        {
            PropertyId = property.Id,
            ComparableIds = compIds,
            RehabLineItems = BuildRehabLineItems(),
            LenderId = lender.Id,
            UseDefaultLoanRate = false
        };

        var result = await service.CreateEvaluationAsync(dto, "user-1");

        Assert.Equal(lender.Id, result.LenderId);
        Assert.True(result.OriginationFeeCost > 0);
        Assert.True(result.LoanServiceFeeCost > 0);
        Assert.True(result.TotalFinancingCosts > 0);
    }

    [Fact]
    public async Task CreateEvaluationAsync_IgnoresLenderSelection_WhenUseDefaultLoanRateIsTrue()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var property = await SeedPropertyAsync(context);
        var compIds = await SeedComparablesAsync(context, property.Id);
        await SeedSettingsAsync(context, "user-1", defaultLoanRate: 0.0);

        var lender = new Lender
        {
            UserId = "user-1",
            Name = "Should Be Ignored",
            AnnualRate = 0.2,
            OriginationFee = 0.03,
            LoanServiceFee = 0.01
        };
        context.Lenders.Add(lender);
        await context.SaveChangesAsync();

        var dto = new CreateEvaluationDto
        {
            PropertyId = property.Id,
            ComparableIds = compIds,
            RehabLineItems = BuildRehabLineItems(),
            LenderId = lender.Id,
            UseDefaultLoanRate = true
        };

        var result = await service.CreateEvaluationAsync(dto, "user-1");

        Assert.Null(result.LenderId);
        Assert.Equal(0, result.TotalInterest);
        Assert.Equal(0, result.OriginationFeeCost);
        Assert.Equal(0, result.LoanServiceFeeCost);
    }

    [Fact]
    public async Task CreateEvaluationAsync_LowersMaxOffer_WhenUsingHigherFixedProfitTarget()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var property = await SeedPropertyAsync(context);
        var compIds = await SeedComparablesAsync(context, property.Id);

        await SeedSettingsAsync(
            context,
            "user-1",
            profitTargetType: ProfitTargetType.PercentageOfArv,
            profitTargetValue: 0.10);

        var baseDto = new CreateEvaluationDto
        {
            PropertyId = property.Id,
            ComparableIds = compIds,
            RehabLineItems = BuildRehabLineItems(),
            UseDefaultLoanRate = true
        };

        var percentageTargetResult = await service.CreateEvaluationAsync(baseDto, "user-1");

        var settings = await context.DealSettings.FirstAsync(s => s.UserId == "user-1");
        settings.ProfitTargetType = ProfitTargetType.FixedAmount;
        settings.ProfitTargetValue = 50000;
        context.DealSettings.Update(settings);
        await context.SaveChangesAsync();

        var fixedTargetResult = await service.CreateEvaluationAsync(baseDto, "user-1");

        Assert.NotNull(percentageTargetResult.MaxOffer);
        Assert.NotNull(fixedTargetResult.MaxOffer);
        Assert.True(fixedTargetResult.MaxOffer < percentageTargetResult.MaxOffer);
    }

    private static async Task<Property> SeedPropertyAsync(DealEvaluatorContext context)
    {
        var property = new Property
        {
            UserId = "user-1",
            Address = "100 Test St",
            City = "Austin",
            State = "TX",
            ZipCode = "78701",
            PropertyType = PropertyTypes.SingleFamily,
            PropertyConditions = PropertyConditions.MinorRepairs,
            Price = 250000,
            Sqft = 1500
        };

        context.Properties.Add(property);
        await context.SaveChangesAsync();
        return property;
    }

    private static async Task<List<int>> SeedComparablesAsync(DealEvaluatorContext context, int propertyId)
    {
        var comparables = new List<Comparable>
        {
            new()
            {
                PropertyId = propertyId,
                Address = "101 Test St",
                City = "Austin",
                State = "TX",
                ZipCode = "78701",
                Price = 300000,
                Source = "Test",
                ListingUrl = string.Empty,
                ComparableType = ComparableType.Arv
            },
            new()
            {
                PropertyId = propertyId,
                Address = "102 Test St",
                City = "Austin",
                State = "TX",
                ZipCode = "78701",
                Price = 320000,
                Source = "Test",
                ListingUrl = string.Empty,
                ComparableType = ComparableType.Arv
            }
        };

        context.Comparables.AddRange(comparables);
        await context.SaveChangesAsync();
        return comparables.Select(c => c.Id).ToList();
    }

    private static async Task SeedSettingsAsync(
        DealEvaluatorContext context,
        string userId,
        double? defaultLoanRate = null,
        ProfitTargetType? profitTargetType = null,
        double? profitTargetValue = null)
    {
        var settings = new DealSettings
        {
            UserId = userId,
            SellingAgentCommission = 0.06,
            SellingClosingCosts = 0.02,
            BuyingClosingCosts = 0.02,
            AnnualPropertyTaxRate = 0.012,
            MonthlyInsurance = 150,
            MonthlyUtilities = 200,
            DefaultHoldingMonths = 4,
            DownPaymentPercentage = 0.2,
            DefaultLoanRate = defaultLoanRate ?? 0.12,
            ProfitTargetType = profitTargetType ?? ProfitTargetType.PercentageOfArv,
            ProfitTargetValue = profitTargetValue ?? 0.15,
            ContingencyPercentage = 0.1
        };

        context.DealSettings.Add(settings);
        await context.SaveChangesAsync();
    }

    private static List<CreateRehabLineItemDto> BuildRehabLineItems()
    {
        return new List<CreateRehabLineItemDto>
        {
            new()
            {
                LineItemType = RehabLineItemType.Kitchen,
                Condition = RehabCondition.Moderate,
                Quantity = 1,
                UnitCost = 15000
            },
            new()
            {
                LineItemType = RehabLineItemType.Bathroom,
                Condition = RehabCondition.Moderate,
                Quantity = 1,
                UnitCost = 8000
            }
        };
    }

    private static PropertyService CreateService(DealEvaluatorContext context)
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        var propertyRepository = new PropertyRepository(context);
        var evaluationRepository = new EvaluationRepository(context);
        var lenderRepository = new LenderRepository(context);
        var dealSettingsService = new DealSettingsService(new DealSettingsRepository(context));
        var rehabRepository = new RehabCostTemplateRepository(context);

        return new PropertyService(
            propertyRepository,
            evaluationRepository,
            new NoopCompService(),
            rehabRepository,
            dealSettingsService,
            lenderRepository,
            mapper,
            new NoopHttpClientFactory());
    }

    private static DealEvaluatorContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DealEvaluatorContext>()
            .UseInMemoryDatabase($"DealEvaluatorEvaluationTests-{Guid.NewGuid()}")
            .Options;

        return new DealEvaluatorContext(options);
    }

    private sealed class NoopCompService : ICompService
    {
        public Task<List<Application.DTOs.Zillow.ZillowProperty>> FindComparablesAsync(
            PropertyTypes propertyType,
            int? bedrooms,
            int? bathrooms,
            int? sqft,
            string zipCode,
            string? subjectPropertyAddress = null)
        {
            throw new NotSupportedException("Comp service is not used in these CreateEvaluationAsync tests.");
        }
    }

    private sealed class NoopHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
