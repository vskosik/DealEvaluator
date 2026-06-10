using DealEvaluator.Application.DTOs.Comparable;
using DealEvaluator.Application.DTOs.Zillow;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Application.Services;
using DealEvaluator.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace DealEvaluator.Tests;

public class CompServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static CompService CreateService(IEnumerable<ZillowProperty> marketData) =>
        new(new StubMarketDataService(marketData.ToList()), NullLogger<CompService>.Instance);

    // Fluent builder for ZillowProperty test data
    private static ZillowProperty Make(
        string type      = "SINGLE_FAMILY",
        int    price     = 300_000,
        int?   sqft      = 1_500,
        int?   beds      = 3,
        float? baths     = 2f,
        double? lat      = null,
        double? lon      = null,
        long?  dateSoldMs = null,
        string zpid      = "0",
        string address   = "100 Comp St") =>
        new()
        {
            Id               = zpid,
            PropertyType     = type,
            Price            = price,
            LivingArea       = sqft,
            Bedrooms         = beds,
            Bathrooms        = baths,
            Latitude         = lat,
            Longitude        = lon,
            DateSoldTimestamp = dateSoldMs,
            Address          = address
        };

    // Unix-ms timestamp N months before now (using Zillow's 30.44 days/month)
    private static long MonthsAgoMs(double months) =>
        DateTimeOffset.UtcNow.AddDays(-months * 30.44).ToUnixTimeMilliseconds();

    private sealed class StubMarketDataService : IMarketDataService
    {
        private readonly List<ZillowProperty> _data;
        public StubMarketDataService(List<ZillowProperty> data) => _data = data;
        public Task<List<ZillowProperty>> GetMarketDataForZipCodeAsync(string z, string h, string k = "") => Task.FromResult(_data);
        public Task<List<ZillowProperty>> RefreshMarketDataAsync(string z, string h, string k = "") => Task.FromResult(_data);
        public Task<bool> HasFreshDataAsync(string z, string h, string k = "") => Task.FromResult(true);
    }

    private const string Zip = "07001";
    private const double SubjectLat = 40.7128;
    private const double SubjectLon = -74.0060;
    private static readonly PropertyTypes SFR = PropertyTypes.SingleFamily;

    // =========================================================================
    // Market data availability
    // =========================================================================

    [Fact]
    public async Task NoMarketData_Returns_Insufficient()
    {
        var svc = CreateService([]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(CompConfidence.Insufficient, result.Confidence);
        Assert.Empty(result.Comparables);
    }

    [Fact]
    public async Task NoMatchingPropertyType_Returns_Insufficient()
    {
        var svc = CreateService([Make(type: "CONDO")]); // subject is SFR
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(CompConfidence.Insufficient, result.Confidence);
        Assert.Equal(0, result.CandidatePoolSize);
    }

    [Fact]
    public async Task AllCandidatesFailHardGates_Returns_Insufficient_WithPoolCount()
    {
        // 100 % sqft deviation — every property gets excluded by the sqft gate
        var props = Enumerable.Range(0, 5).Select(_ => Make(sqft: 3_000)).ToList();
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(CompConfidence.Insufficient, result.Confidence);
        Assert.Equal(5, result.CandidatePoolSize); // passed type/price filter, failed scoring gate
    }

    [Fact]
    public async Task ZeroPriceProperties_ExcludedFromCandidatePool()
    {
        var props = new List<ZillowProperty> { Make(price: 0), Make(price: -1) };
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(0, result.CandidatePoolSize);
    }

    [Fact]
    public async Task CandidatePoolSize_ExcludesWrongTypeAndZeroPrice()
    {
        var props = new List<ZillowProperty>
        {
            Make(),                  // valid SFR
            Make(),                  // valid SFR
            Make(type: "CONDO"),     // wrong type
            Make(price: 0)           // invalid price
        };
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(2, result.CandidatePoolSize);
    }

    // =========================================================================
    // Subject exclusion (PreFilter)
    // =========================================================================

    [Fact]
    public async Task SubjectExcluded_ByExactAddressMatch()
    {
        var svc = CreateService([Make(address: "100 Main St")]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip,
            subjectPropertyAddress: "100 Main St");
        Assert.Equal(0, result.CandidatePoolSize);
    }

    [Fact]
    public async Task SubjectExcluded_AddressNormalization_FullSuffixMatchesAbbreviation()
    {
        // Subject uses "Street"; candidate uses "St" — normalization must unify them
        var svc = CreateService([Make(address: "123 Main St")]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip,
            subjectPropertyAddress: "123 Main Street");
        Assert.Equal(0, result.CandidatePoolSize);
    }

    [Fact]
    public async Task SubjectExcluded_AddressNormalization_PrefixMatch()
    {
        // Zillow candidate address includes city/state; subject is just the street line
        var svc = CreateService([Make(address: "123 Main St, Newark, NJ 07102")]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip,
            subjectPropertyAddress: "123 Main St");
        Assert.Equal(0, result.CandidatePoolSize);
    }

    [Fact]
    public async Task SubjectExcluded_ByZpid_EvenWhenAddressDiffers()
    {
        var svc = CreateService([Make(zpid: "abc123", address: "999 Unrelated Ave")]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip,
            subjectPropertyAddress: "100 Subject St", subjectZpid: "abc123");
        Assert.Equal(0, result.CandidatePoolSize);
    }

    [Fact]
    public async Task SubjectNotExcluded_WhenCandidateZpidDiffers_AddressIgnored()
    {
        // Same address as subject but different ZPID → a genuinely different property
        var svc = CreateService([Make(zpid: "abc123", address: "100 Subject St")]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip,
            subjectPropertyAddress: "100 Subject St", subjectZpid: "xyz999");
        Assert.Equal(1, result.CandidatePoolSize); // ZPID mismatch → not excluded
    }

    // =========================================================================
    // Hard gates
    // =========================================================================

    [Fact]
    public async Task SqftDeviation_Above35Pct_CandidateExcluded()
    {
        // Target 1 000 sqft; 1 361 = 36.1 % deviation
        var svc = CreateService([Make(sqft: 1_361)]);
        var result = await svc.FindComparablesAsync(SFR, null, null, 1_000, Zip);
        Assert.Empty(result.Comparables);
    }

    [Fact]
    public async Task SqftDeviation_ExactlyAt35Pct_Included_WithSqftScoreOfZero()
    {
        // 1 000 → 1 350: deviation exactly 0.35 → included, sqftScore = 0
        var svc = CreateService([Make(sqft: 1_350)]);
        var result = await svc.FindComparablesAsync(SFR, null, null, 1_000, Zip);
        Assert.Single(result.Comparables);
        Assert.Equal(0m, result.Comparables[0].SqftScore);
    }

    [Fact]
    public async Task BedDiff_Above2_CandidateExcluded()
    {
        var svc = CreateService([Make(beds: 6)]); // target 3 → diff 3
        var result = await svc.FindComparablesAsync(SFR, 3, null, null, Zip);
        Assert.Empty(result.Comparables);
    }

    [Fact]
    public async Task BedDiff_ExactlyAt2_Included_WithLowBedScore()
    {
        var svc = CreateService([Make(beds: 5)]); // target 3 → diff 2
        var result = await svc.FindComparablesAsync(SFR, 3, null, null, Zip);
        Assert.Single(result.Comparables);
        Assert.Equal(0.15m, result.Comparables[0].BedScore);
    }

    [Fact]
    public async Task BathDiff_Above2_CandidateExcluded()
    {
        var svc = CreateService([Make(baths: 5f)]); // target 2 → diff 3
        var result = await svc.FindComparablesAsync(SFR, null, 2m, null, Zip);
        Assert.Empty(result.Comparables);
    }

    [Fact]
    public async Task BathDiff_ExactlyAt2_Included()
    {
        var svc = CreateService([Make(baths: 4f)]); // target 2 → diff 2
        var result = await svc.FindComparablesAsync(SFR, null, 2m, null, Zip);
        Assert.Single(result.Comparables);
    }

    // =========================================================================
    // Missing data scoring
    // =========================================================================

    [Fact]
    public async Task MissingCandidateSqft_ScoresMissingDataScore_NotExcluded()
    {
        var svc = CreateService([Make(sqft: null)]);
        var result = await svc.FindComparablesAsync(SFR, null, null, 1_500, Zip);
        Assert.Single(result.Comparables);
        Assert.Equal(0.25m, result.Comparables[0].SqftScore);
        Assert.True(result.Comparables[0].HasMissingData);
    }

    [Fact]
    public async Task MissingCandidateBeds_ScoresMissingDataScore_NotExcluded()
    {
        var svc = CreateService([Make(beds: null)]);
        var result = await svc.FindComparablesAsync(SFR, 3, null, null, Zip);
        Assert.Single(result.Comparables);
        Assert.Equal(0.25m, result.Comparables[0].BedScore);
        Assert.True(result.Comparables[0].HasMissingData);
    }

    [Fact]
    public async Task MissingCandidateBaths_ScoresMissingDataScore_NotExcluded()
    {
        var svc = CreateService([Make(baths: null)]);
        var result = await svc.FindComparablesAsync(SFR, null, 2m, null, Zip);
        Assert.Single(result.Comparables);
        Assert.Equal(0.25m, result.Comparables[0].BathScore);
        Assert.True(result.Comparables[0].HasMissingData);
    }

    [Fact]
    public async Task MissingDateSold_RecencyDimensionDropped_WeightRedistributes()
    {
        // DateSold null → recencyScore is null, NOT 0.25; weight redistributes
        var svc = CreateService([Make(dateSoldMs: null)]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip);
        var comp = result.Comparables[0];
        Assert.Null(comp.RecencyScore);
        Assert.False(comp.HasMissingData); // missing target data, not missing candidate data
    }

    [Fact]
    public async Task MissingCandidateCoords_WhenSubjectHasCoords_ScoresMissingDataScore()
    {
        var svc = CreateService([Make(lat: null, lon: null)]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip,
            subjectLatitude: SubjectLat, subjectLongitude: SubjectLon);
        var comp = result.Comparables[0];
        Assert.Equal(0.25m, comp.DistanceScore);
        Assert.Null(comp.DistanceMiles);
        Assert.True(comp.HasMissingData);
    }

    [Fact]
    public async Task MissingSubjectCoords_DistanceDimensionSkippedForAll()
    {
        // Candidate has coords but subject doesn't → whole dimension absent
        var svc = CreateService([Make(lat: SubjectLat, lon: SubjectLon)]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip,
            subjectLatitude: null, subjectLongitude: null);
        var comp = result.Comparables[0];
        Assert.Null(comp.DistanceScore);
        Assert.False(comp.HasMissingData);
    }

    // =========================================================================
    // Weight renormalization
    // =========================================================================

    [Fact]
    public async Task AllTargetsNull_NoCoords_NoDates_ScoresHalfAsFallback()
    {
        // weightTotal = 0 → fallback 0.5
        var svc = CreateService([Make(dateSoldMs: null, lat: null, lon: null)]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip);
        Assert.Equal(0.5m, result.Comparables[0].Score);
    }

    [Fact]
    public async Task PerfectMatch_AllDimensions_ScoresNearOne()
    {
        // dateSoldMs = now (0 months ago) → recencyScore ≈ 1.0 → all dims ≈ 1.0
        var prop = Make(sqft: 1_500, beds: 3, baths: 2f,
                        lat: SubjectLat, lon: SubjectLon,
                        dateSoldMs: MonthsAgoMs(0));
        var svc = CreateService([prop]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip,
            subjectLatitude: SubjectLat, subjectLongitude: SubjectLon);
        Assert.True(result.Comparables[0].Score >= 0.98m);
        Assert.False(result.Comparables[0].HasMissingData);
    }

    [Fact]
    public async Task MissingRecency_WeightRedistributes_HigherScoreThanMissingDataScore()
    {
        // With all other dimensions perfect and recency weight redistributed,
        // the score should be 1.0, not 0.25 (MissingDataScore)
        var noDate  = Make(sqft: 1_500, beds: 3, baths: 2f, dateSoldMs: null,    zpid: "nodate");
        var hasDate = Make(sqft: 1_500, beds: 3, baths: 2f, dateSoldMs: MonthsAgoMs(23), zpid: "old");
        var svc = CreateService([noDate, hasDate]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        var noDateScore  = result.Comparables.First(c => c.Property.Id == "nodate").Score;
        var hasDateScore = result.Comparables.First(c => c.Property.Id == "old").Score;
        Assert.True(noDateScore > hasDateScore,
            $"No-date ({noDateScore}) should outscore old-sale ({hasDateScore}) when recency redistributes vs decays");
    }

    // =========================================================================
    // Recency scoring
    // =========================================================================

    [Fact]
    public async Task SoldToday_RecencyScoreIsOne()
    {
        var svc = CreateService([Make(dateSoldMs: MonthsAgoMs(0))]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip);
        Assert.True(result.Comparables[0].RecencyScore >= 0.99m);
    }

    [Fact]
    public async Task Sold12MonthsAgo_RecencyScoreIsApproximatelyHalf()
    {
        var svc = CreateService([Make(dateSoldMs: MonthsAgoMs(12))]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip);
        var score = result.Comparables[0].RecencyScore!.Value;
        Assert.True(score is >= 0.45m and <= 0.55m, $"Expected ~0.50, got {score}");
    }

    [Fact]
    public async Task Sold24MonthsAgo_RecencyScoreIsZero()
    {
        var svc = CreateService([Make(dateSoldMs: MonthsAgoMs(24))]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip);
        Assert.Equal(0m, result.Comparables[0].RecencyScore);
    }

    [Fact]
    public async Task SoldBeyondMaxRecency_RecencyScoreClampedAtZero()
    {
        var svc = CreateService([Make(dateSoldMs: MonthsAgoMs(36))]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip);
        Assert.Equal(0m, result.Comparables[0].RecencyScore);
    }

    [Fact]
    public async Task RecentSale_RanksAboveOlderSale_WhenOtherDimsEqual()
    {
        var recent = Make(sqft: 1_500, beds: 3, baths: 2f, dateSoldMs: MonthsAgoMs(2),  zpid: "recent");
        var old    = Make(sqft: 1_500, beds: 3, baths: 2f, dateSoldMs: MonthsAgoMs(20), zpid: "old");
        var svc = CreateService([old, recent]); // intentionally pass old first
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal("recent", result.Comparables[0].Property.Id);
    }

    // =========================================================================
    // Distance scoring
    // =========================================================================

    [Fact]
    public async Task SameLocation_DistanceScoreIsOne_DistanceMilesIsZero()
    {
        var svc = CreateService([Make(lat: SubjectLat, lon: SubjectLon)]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip,
            subjectLatitude: SubjectLat, subjectLongitude: SubjectLon);
        Assert.Equal(1.0m, result.Comparables[0].DistanceScore);
        Assert.Equal(0.0m, result.Comparables[0].DistanceMiles);
    }

    [Fact]
    public async Task FarCandidate_DistanceScoreClampedAtZero()
    {
        // ~10 miles north — well beyond MaxDistanceMiles (2.0)
        var farLat = SubjectLat + 10.0 / 69.0;
        var svc = CreateService([Make(lat: farLat, lon: SubjectLon)]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip,
            subjectLatitude: SubjectLat, subjectLongitude: SubjectLon);
        Assert.Equal(0m, result.Comparables[0].DistanceScore);
    }

    [Fact]
    public async Task NearbyCandidate_RanksAboveDistantCandidate()
    {
        var farLat  = SubjectLat + 5.0 / 69.0; // ~5 miles north
        var nearby  = Make(sqft: 1_500, lat: SubjectLat, lon: SubjectLon, zpid: "near");
        var distant = Make(sqft: 1_500, lat: farLat,     lon: SubjectLon, zpid: "far");
        var svc = CreateService([distant, nearby]); // distant first
        var result = await svc.FindComparablesAsync(SFR, null, null, 1_500, Zip,
            subjectLatitude: SubjectLat, subjectLongitude: SubjectLon);
        Assert.Equal("near", result.Comparables[0].Property.Id);
    }

    [Fact]
    public async Task DistanceMiles_IsPopulated_WhenBothCoordsPresent()
    {
        var slightlyNorth = SubjectLat + 0.5 / 69.0; // ~0.5 miles north
        var svc = CreateService([Make(lat: slightlyNorth, lon: SubjectLon)]);
        var result = await svc.FindComparablesAsync(SFR, null, null, null, Zip,
            subjectLatitude: SubjectLat, subjectLongitude: SubjectLon);
        var miles = result.Comparables[0].DistanceMiles;
        Assert.NotNull(miles);
        Assert.True(miles > 0m && miles < 1m, $"Expected < 1 mile, got {miles}");
    }

    // =========================================================================
    // MAD $/sqft outlier trim
    // =========================================================================

    [Fact]
    public async Task PoolUnder4_NoOutlierTrimming()
    {
        // 3 properties; one is a huge outlier — but pool < 4, so no trim
        var props = new List<ZillowProperty>
        {
            Make(price:   150_000, sqft: 1_500, zpid: "1"), // $100/sqft
            Make(price:   150_000, sqft: 1_500, zpid: "2"), // $100/sqft
            Make(price: 1_500_000, sqft: 1_500, zpid: "X")  // $1 000/sqft — would be outlier
        };
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, null, null, 1_500, Zip);
        Assert.Equal(0, result.OutliersRemoved);
        Assert.Equal(3, result.Comparables.Count);
    }

    [Fact]
    public async Task MadIsZero_NoOutlierTrimming()
    {
        // All same $/sqft → every absolute deviation is 0 → MAD = 0 → skip trim
        var props = Enumerable.Range(1, 5)
            .Select(i => Make(price: 150_000, sqft: 1_500, zpid: i.ToString()))
            .ToList();
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, null, null, 1_500, Zip);
        Assert.Equal(0, result.OutliersRemoved);
        Assert.Equal(5, result.Comparables.Count);
    }

    [Fact]
    public async Task ClearPriceOutlier_TrimmedFromSelection()
    {
        // $/sqft values: 95, 97, 100, 103, 105, 500
        // Median = 101.5, MAD = 4.0  →  |500 - 101.5| / 4.0 = 99.6 >> 2.5 → outlier
        var props = new List<ZillowProperty>
        {
            Make(price:   142_500, sqft: 1_500, zpid: "1"),  // $ 95/sqft
            Make(price:   145_500, sqft: 1_500, zpid: "2"),  // $ 97/sqft
            Make(price:   150_000, sqft: 1_500, zpid: "3"),  // $100/sqft
            Make(price:   154_500, sqft: 1_500, zpid: "4"),  // $103/sqft
            Make(price:   157_500, sqft: 1_500, zpid: "5"),  // $105/sqft
            Make(price:   750_000, sqft: 1_500, zpid: "X"),  // $500/sqft — outlier
        };
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, null, null, 1_500, Zip);
        Assert.Equal(1, result.OutliersRemoved);
        Assert.DoesNotContain(result.Comparables, c => c.Property.Id == "X");
        Assert.Equal(5, result.Comparables.Count);
    }

    [Fact]
    public async Task MultipleOutliers_AllTrimmed()
    {
        // Two extreme outliers on opposite ends of the $/sqft distribution
        var props = new List<ZillowProperty>
        {
            Make(price:   150_000, sqft: 1_500, zpid: "1"),  // $100/sqft
            Make(price:   151_500, sqft: 1_500, zpid: "2"),  // $101/sqft
            Make(price:   150_750, sqft: 1_500, zpid: "3"),  // $100.5/sqft
            Make(price:   151_125, sqft: 1_500, zpid: "4"),  // $100.75/sqft
            Make(price:   149_250, sqft: 1_500, zpid: "5"),  // $ 99.5/sqft
            Make(price: 1_500_000, sqft: 1_500, zpid: "Hi"), // $1 000/sqft — outlier
            Make(price:     7_500, sqft: 1_500, zpid: "Lo"), // $  5/sqft   — outlier
        };
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, null, null, 1_500, Zip);
        Assert.True(result.OutliersRemoved >= 2);
        Assert.DoesNotContain(result.Comparables, c => c.Property.Id is "Hi" or "Lo");
    }

    [Fact]
    public async Task PropertyWithoutSqft_ExcludedFromMadCalc_NotTrimmedAsOutlier()
    {
        // 5 normal comps + 1 with no sqft (can't compute $/sqft, excluded from MAD calc)
        var props = new List<ZillowProperty>
        {
            Make(price: 150_000, sqft: 1_500, zpid: "1"),
            Make(price: 150_000, sqft: 1_500, zpid: "2"),
            Make(price: 150_000, sqft: 1_500, zpid: "3"),
            Make(price: 150_000, sqft: 1_500, zpid: "4"),
            Make(price: 150_000, sqft: 1_500, zpid: "5"),
            Make(price: 150_000, sqft: null,  zpid: "no-sqft"),
        };
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, null, null, 1_500, Zip);
        // Same $/sqft for all 5 normal ones → MAD = 0 → no trim
        Assert.Equal(0, result.OutliersRemoved);
    }

    // =========================================================================
    // Confidence grading
    // =========================================================================

    [Fact]
    public async Task ZeroComps_ConfidenceInsufficient()
    {
        var svc = CreateService([]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(CompConfidence.Insufficient, result.Confidence);
    }

    [Fact]
    public async Task OneComp_ConfidenceLow()
    {
        var svc = CreateService([Make()]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(CompConfidence.Low, result.Confidence);
        Assert.Single(result.Comparables);
    }

    [Fact]
    public async Task TwoComps_ConfidenceLow()
    {
        var svc = CreateService([Make(zpid: "1"), Make(zpid: "2")]);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(CompConfidence.Low, result.Confidence);
    }

    [Fact]
    public async Task ThreePerfectComps_NoMissingData_RecentSales_ConfidenceHigh()
    {
        var props = Enumerable.Range(1, 3)
            .Select(i => Make(zpid: i.ToString(), sqft: 1_500, beds: 3, baths: 2f,
                              dateSoldMs: MonthsAgoMs(1)))
            .ToList();
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(CompConfidence.High, result.Confidence);
        Assert.All(result.Comparables, c => Assert.False(c.HasMissingData));
    }

    [Fact]
    public async Task ThreeComps_WithMissingData_CannotBeHigh()
    {
        // Score is high but missing baths → HasMissingData → capped at Medium
        var props = Enumerable.Range(1, 3)
            .Select(i => Make(zpid: i.ToString(), sqft: 1_500, beds: 3, baths: null,
                              dateSoldMs: MonthsAgoMs(1)))
            .ToList();
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.NotEqual(CompConfidence.High, result.Confidence);
        Assert.All(result.Comparables, c => Assert.True(c.HasMissingData));
    }

    [Fact]
    public async Task ThreeComps_ModerateScore_ConfidenceMedium()
    {
        // ±1 bed + 12-month-old sales → score ≈ 0.75 (Medium band: 0.55–0.80)
        var props = Enumerable.Range(1, 3)
            .Select(i => Make(zpid: i.ToString(), sqft: 1_500, beds: 4, baths: 2f,
                              dateSoldMs: MonthsAgoMs(12)))
            .ToList();
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(CompConfidence.Medium, result.Confidence);
    }

    [Fact]
    public async Task ThreeComps_LowScore_ConfidenceLow()
    {
        // Missing sqft + ±2 beds + missing baths + 23-month-old sales → score ≈ 0.17
        var props = Enumerable.Range(1, 3)
            .Select(i => Make(zpid: i.ToString(), sqft: null, beds: 5, baths: null,
                              dateSoldMs: MonthsAgoMs(23)))
            .ToList();
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal(CompConfidence.Low, result.Confidence);
    }

    // =========================================================================
    // Selection and ranking
    // =========================================================================

    [Fact]
    public async Task Selection_LimitedToFiveComps()
    {
        var props = Enumerable.Range(1, 10).Select(i => Make(zpid: i.ToString())).ToList();
        var svc = CreateService(props);
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.True(result.Comparables.Count <= 5);
    }

    [Fact]
    public async Task BetterMatch_RanksFirst()
    {
        var exact = Make(sqft: 1_500, beds: 3, baths: 2f, zpid: "exact");
        var loose = Make(sqft: 2_000, beds: 5, baths: 4f, zpid: "loose"); // ±33% sqft, ±2 beds
        var svc = CreateService([loose, exact]); // loose listed first in input
        var result = await svc.FindComparablesAsync(SFR, 3, 2m, 1_500, Zip);
        Assert.Equal("exact", result.Comparables[0].Property.Id);
        Assert.True(result.Comparables[0].Score > result.Comparables[1].Score);
    }

}
