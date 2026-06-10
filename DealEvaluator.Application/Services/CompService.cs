using DealEvaluator.Application.DTOs.Comparable;
using DealEvaluator.Application.DTOs.Zillow;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Application.Mappings;
using DealEvaluator.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DealEvaluator.Application.Services;

/// <summary>
/// Finds comparable properties using weighted similarity scoring rather than
/// sequential criteria-widening tiers. All candidates are scored at once and
/// the genuinely closest matches win; price outliers are trimmed by $/sqft
/// before final selection.
/// </summary>
public class CompService : ICompService
{
    private readonly IMarketDataService _marketDataService;
    private readonly ILogger<CompService> _logger;

    // ---- Tunables ----------------------------------------------------------

    // Hard gates: candidates outside these bounds are never considered.
    private const decimal MaxSqftDeviation = 0.35m;   // ±35% sqft
    private const int MaxBedDeviation = 2;            // ±2 beds
    private const decimal MaxBathDeviation = 2.0m;    // ±2 baths

    // Scoring weights (renormalized over available dimensions per candidate).
    // Dimensions with null target or null candidate data are dropped and
    // remaining weights are renormalized, so a missing dimension redistributes
    // its weight rather than zeroing the score.
    private const decimal WeightSqft = 0.35m;
    private const decimal WeightBeds = 0.20m;
    private const decimal WeightBaths = 0.10m;
    private const decimal WeightRecency = 0.25m;
    private const decimal WeightDistance = 0.10m;

    // A candidate with known-but-missing data on a dimension (e.g. no lat/long
    // while the subject has coordinates) scores this instead of being excluded.
    private const decimal MissingDataScore = 0.25m;

    // Recency: linear decay from 1.0 (just sold) to 0.0 at MaxRecencyMonths.
    // Beyond that the score floors at 0 (not excluded — hard gates don't apply).
    private const decimal MaxRecencyMonths = 24m;

    // Distance: linear decay from 1.0 (same location) to 0.0 at MaxDistanceMiles.
    private const decimal MaxDistanceMiles = 2.0m;

    // Outlier trim: drop comps whose $/sqft deviates from the pool median
    // by more than this many median-absolute-deviations.
    private const decimal MadCutoff = 2.5m;

    // Selection sizes.
    private const int ScoringPoolSize = 12;  // top-N scored comps eligible for trim
    private const int MaxComps = 5;

    // Confidence thresholds.
    private const decimal HighConfidenceMinScore = 0.80m;
    private const decimal MediumConfidenceMinScore = 0.55m;

    public CompService(
        IMarketDataService marketDataService,
        ILogger<CompService> logger)
    {
        _marketDataService = marketDataService;
        _logger = logger;
    }

    public async Task<CompSearchResult> FindComparablesAsync(
        PropertyTypes propertyType,
        int? bedrooms,
        decimal? bathrooms,
        int? sqft,
        string zipCode,
        string? subjectPropertyAddress = null,
        string? subjectZpid = null,
        double? subjectLatitude = null,
        double? subjectLongitude = null,
        string searchKeyword = "")
    {
        _logger.LogInformation(
            "Finding comparables for {PropertyType} {Beds}bd/{Baths}ba {Sqft}sqft in {ZipCode}",
            propertyType, bedrooms, bathrooms, sqft, zipCode);

        var homeType = PropertyTypeMapper.ToZillowHomeTypeString(propertyType);
        var marketData = await _marketDataService.GetMarketDataForZipCodeAsync(zipCode, homeType, searchKeyword);

        if (marketData == null || marketData.Count == 0)
        {
            _logger.LogWarning("No market data returned for zip {ZipCode}", zipCode);
            return new CompSearchResult
            {
                Confidence = CompConfidence.Insufficient,
                Notes = $"No market data available for zip code {zipCode}."
            };
        }

        // ---- 1. Pre-filter: type match, valid price, exclude subject -------
        var candidates = PreFilter(marketData, propertyType, subjectPropertyAddress, subjectZpid);

        if (candidates.Count == 0)
        {
            return new CompSearchResult
            {
                Confidence = CompConfidence.Insufficient,
                CandidatePoolSize = 0,
                Notes = $"No {propertyType} properties with valid prices in {zipCode}."
            };
        }

        // ---- 2. Score every candidate, apply hard gates ---------------------
        var now = DateTime.UtcNow;
        var scored = candidates
            .Select(p => ScoreCandidate(p, bedrooms, bathrooms, sqft, subjectLatitude, subjectLongitude, now))
            .Where(s => s != null)
            .Cast<ScoredComparable>()
            .OrderByDescending(s => s.Score)
            .ToList();

        if (scored.Count == 0)
        {
            return new CompSearchResult
            {
                Confidence = CompConfidence.Insufficient,
                CandidatePoolSize = candidates.Count,
                Notes = "Candidates existed but none passed similarity gates " +
                        $"(±{MaxSqftDeviation:P0} sqft, ±{MaxBedDeviation} beds)."
            };
        }

        // ---- 3. Trim $/sqft outliers from the top of the pool ---------------
        var pool = scored.Take(ScoringPoolSize).ToList();
        var (trimmed, outliersRemoved) = TrimPriceOutliers(pool);

        // ---- 4. Final selection + confidence grading ------------------------
        var selected = trimmed.Take(MaxComps).ToList();
        var confidence = GradeConfidence(selected);

        _logger.LogInformation(
            "Selected {Count} comps (confidence: {Confidence}, pool: {Pool}, outliers removed: {Outliers}, top score: {Score:F2})",
            selected.Count, confidence, candidates.Count, outliersRemoved,
            selected.FirstOrDefault()?.Score ?? 0);

        return new CompSearchResult
        {
            Comparables = selected,
            Confidence = confidence,
            CandidatePoolSize = candidates.Count,
            OutliersRemoved = outliersRemoved
        };
    }

    // -------------------------------------------------------------------------

    private List<ZillowProperty> PreFilter(
        List<ZillowProperty> properties,
        PropertyTypes propertyType,
        string? subjectAddress,
        string? subjectZpid)
    {
        var targetType = MapPropertyTypeToZillowType(propertyType);
        var normalizedSubject = NormalizeAddress(subjectAddress);

        return properties.Where(p =>
                p.PropertyType != null &&
                p.PropertyType.Equals(targetType, StringComparison.OrdinalIgnoreCase) &&
                p.Price is > 0 &&
                !IsSubjectProperty(p, subjectZpid, normalizedSubject))
            .ToList();
    }

    private static bool IsSubjectProperty(ZillowProperty p, string? subjectZpid, string? normalizedSubjectAddress)
    {
        if (subjectZpid != null && p.Id != null)
            return string.Equals(p.Id, subjectZpid, StringComparison.OrdinalIgnoreCase);

        return IsSameAddress(NormalizeAddress(p.Address), normalizedSubjectAddress);
    }

    /// <summary>
    /// Scores a candidate 0..1 against the subject. Returns null if a hard gate
    /// fails. Missing data on a known dimension scores low; unknown target
    /// dimensions are dropped and weight redistributes to the rest.
    /// </summary>
    private ScoredComparable? ScoreCandidate(
        ZillowProperty p,
        int? targetBeds, decimal? targetBaths, int? targetSqft,
        double? subjectLat, double? subjectLon,
        DateTime now)
    {
        decimal? sqftScore = null, bedScore = null, bathScore = null;
        decimal? recencyScore = null, distanceScore = null;
        decimal? distanceMiles = null;
        bool hasMissingData = false;

        // ---- Sqft -----------------------------------------------------------
        if (targetSqft.HasValue)
        {
            if (p.LivingArea.HasValue)
            {
                var deviation = Math.Abs(p.LivingArea.Value - targetSqft.Value)
                                / (decimal)targetSqft.Value;
                if (deviation > MaxSqftDeviation) return null; // hard gate
                sqftScore = 1m - (deviation / MaxSqftDeviation);
            }
            else
            {
                sqftScore = MissingDataScore;
                hasMissingData = true;
            }
        }

        // ---- Beds -----------------------------------------------------------
        if (targetBeds.HasValue)
        {
            if (p.Bedrooms.HasValue)
            {
                var diff = Math.Abs(p.Bedrooms.Value - targetBeds.Value);
                if (diff > MaxBedDeviation) return null; // hard gate
                bedScore = diff switch { 0 => 1m, 1 => 0.5m, _ => 0.15m };
            }
            else
            {
                bedScore = MissingDataScore;
                hasMissingData = true;
            }
        }

        // ---- Baths (fractional — Zillow reports 1.5, 2.5, etc.) ------------
        if (targetBaths.HasValue)
        {
            if (p.Bathrooms.HasValue)
            {
                var diff = Math.Abs((decimal)p.Bathrooms.Value - targetBaths.Value);
                if (diff > MaxBathDeviation) return null; // hard gate
                bathScore = diff switch
                {
                    0m => 1m,
                    <= 0.5m => 0.75m,
                    <= 1.0m => 0.50m,
                    _ => 0.15m
                };
            }
            else
            {
                bathScore = MissingDataScore;
                hasMissingData = true;
            }
        }

        // ---- Recency --------------------------------------------------------
        // DateSold null → skip dimension (weight redistributes); the Zillow API
        // returns RecentlySold so most candidates will have this field.
        if (p.DateSold.HasValue)
        {
            var ageMonths = (decimal)(now - p.DateSold.Value).TotalDays / 30.44m;
            recencyScore = Math.Max(0m, 1m - ageMonths / MaxRecencyMonths);
        }
        // else: recencyScore stays null → weight redistributes

        // ---- Distance -------------------------------------------------------
        // Only scored when the subject has coordinates. If subject coords are
        // null we skip the whole dimension for all candidates equally.
        if (subjectLat.HasValue && subjectLon.HasValue)
        {
            if (p.Latitude.HasValue && p.Longitude.HasValue)
            {
                distanceMiles = HaversineDistanceMiles(
                    subjectLat.Value, subjectLon.Value,
                    p.Latitude.Value, p.Longitude.Value);
                distanceScore = Math.Max(0m, 1m - distanceMiles.Value / MaxDistanceMiles);
            }
            else
            {
                distanceScore = MissingDataScore;
                hasMissingData = true;
            }
        }

        // ---- Weighted average -----------------------------------------------
        // Only dimensions with a non-null score contribute; weights renormalize
        // over whatever subset is available.
        decimal weightedSum = 0m, weightTotal = 0m;
        if (sqftScore.HasValue)     { weightedSum += sqftScore.Value     * WeightSqft;     weightTotal += WeightSqft; }
        if (bedScore.HasValue)      { weightedSum += bedScore.Value      * WeightBeds;      weightTotal += WeightBeds; }
        if (bathScore.HasValue)     { weightedSum += bathScore.Value     * WeightBaths;     weightTotal += WeightBaths; }
        if (recencyScore.HasValue)  { weightedSum += recencyScore.Value  * WeightRecency;  weightTotal += WeightRecency; }
        if (distanceScore.HasValue) { weightedSum += distanceScore.Value * WeightDistance; weightTotal += WeightDistance; }

        var score = weightTotal > 0 ? weightedSum / weightTotal : 0.5m;

        return new ScoredComparable
        {
            Property = p,
            Score = Math.Round(score, 4),
            SqftScore = sqftScore,
            BedScore = bedScore,
            BathScore = bathScore,
            RecencyScore = recencyScore,
            DistanceScore = distanceScore,
            DistanceMiles = distanceMiles.HasValue ? Math.Round(distanceMiles.Value, 2) : null,
            HasMissingData = hasMissingData
        };
    }

    /// <summary>
    /// Removes $/sqft outliers using median absolute deviation. Robust to the
    /// small-N pools typical here; skips trimming when N is too small or MAD
    /// degenerates to zero.
    /// </summary>
    private (List<ScoredComparable> trimmed, int removed) TrimPriceOutliers(
        List<ScoredComparable> pool)
    {
        var withPpsf = pool
            .Where(s => s.Property.LivingArea is > 0 && s.Property.Price is > 0)
            .Select(s => (Comp: s, Ppsf: (decimal)s.Property.Price!.Value / s.Property.LivingArea!.Value))
            .ToList();

        // Not enough data to establish a distribution — don't trim.
        if (withPpsf.Count < 4) return (pool, 0);

        var ppsfValues = withPpsf.Select(x => x.Ppsf).OrderBy(x => x).ToList();
        var median = Median(ppsfValues);
        var mad = Median(ppsfValues.Select(v => Math.Abs(v - median)).OrderBy(x => x).ToList());

        if (mad == 0) return (pool, 0); // degenerate distribution, nothing to trim

        var outlierComps = withPpsf
            .Where(x => Math.Abs(x.Ppsf - median) / mad > MadCutoff)
            .Select(x => x.Comp)
            .ToHashSet();

        if (outlierComps.Count == 0) return (pool, 0);

        foreach (var o in outlierComps)
        {
            _logger.LogInformation(
                "Trimmed $/sqft outlier: {Address} at {Price:C0} ({Ppsf:C0}/sqft vs median {Median:C0}/sqft)",
                o.Property.Address, o.Property.Price,
                o.Property.Price!.Value / (decimal)o.Property.LivingArea!.Value, median);
        }

        return (pool.Where(s => !outlierComps.Contains(s)).ToList(), outlierComps.Count);
    }

    private CompConfidence GradeConfidence(List<ScoredComparable> selected)
    {
        if (selected.Count == 0) return CompConfidence.Insufficient;

        var avgScore = selected.Average(s => s.Score);
        var anyMissing = selected.Any(s => s.HasMissingData);

        if (selected.Count >= 3 && avgScore >= HighConfidenceMinScore && !anyMissing)
            return CompConfidence.High;

        if (selected.Count >= 3 && avgScore >= MediumConfidenceMinScore)
            return CompConfidence.Medium;

        return CompConfidence.Low;
    }

    // ---- Helpers -------------------------------------------------------------

    private static decimal Median(List<decimal> sorted)
    {
        var n = sorted.Count;
        return n % 2 == 1
            ? sorted[n / 2]
            : (sorted[n / 2 - 1] + sorted[n / 2]) / 2m;
    }

    /// <summary>
    /// Normalizes an address for comparison: uppercase, punctuation stripped,
    /// common suffixes unified, whitespace collapsed. Prefer comparing by ZPID
    /// if your DTO carries it — that is strictly more reliable than this.
    /// </summary>
    private static string? NormalizeAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        var s = new string(address
                .ToUpperInvariant()
                .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                .ToArray());

        var suffixMap = new Dictionary<string, string>
        {
            ["STREET"] = "ST", ["AVENUE"] = "AVE", ["BOULEVARD"] = "BLVD",
            ["DRIVE"] = "DR", ["ROAD"] = "RD", ["LANE"] = "LN",
            ["COURT"] = "CT", ["PLACE"] = "PL", ["TERRACE"] = "TER",
            ["CIRCLE"] = "CIR", ["PARKWAY"] = "PKWY"
        };

        var tokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => suffixMap.TryGetValue(t, out var abbr) ? abbr : t);

        return string.Join(' ', tokens);
    }

    private static bool IsSameAddress(string? a, string? b)
    {
        if (a == null || b == null) return false;
        // Prefix match handles "123 MAIN ST" vs "123 MAIN ST NEWARK NJ 07102".
        return a == b || a.StartsWith(b + " ") || b.StartsWith(a + " ");
    }

    private static decimal HaversineDistanceMiles(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 3958.8; // Earth radius in miles
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return (decimal)(R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static string MapPropertyTypeToZillowType(PropertyTypes propertyType)
    {
        return propertyType switch
        {
            PropertyTypes.SingleFamily => "SINGLE_FAMILY",
            PropertyTypes.MultiFamily => "MULTI_FAMILY",
            PropertyTypes.Condo => "CONDO",
            PropertyTypes.Townhouse => "TOWNHOUSE",
            _ => throw new ArgumentException($"Unknown property type: {propertyType}")
        };
    }
}