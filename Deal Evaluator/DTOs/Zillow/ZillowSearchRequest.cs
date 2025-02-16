namespace Deal_Evaluator.DTOs.Zillow;

public class ZillowSearchRequest
{
    public required string Location { get; set; } // Address, county, neighborhood, zip code
    public ZillowStatusType StatusType { get; set; } = ZillowStatusType.RecentlySold; // ForSale, RecentlySold
    public ZillowHomeType HomeType { get; set; } = ZillowHomeType.Houses; // "Multi-family", "Houses", etc.
    public ZillowSort Sort { get; set; } = ZillowSort.Newest;
    public int? BathsMin { get; set; }
    public int? BathsMax { get; set; }
    public int? BedsMin { get; set; }
    public int? BedsMax { get; set; }
    public string? DaysOn { get; set; } // 1, 7, 14, 6m, 12m, etc.
    public string? SoldInLast { get; set; } // 1, 7, 14, 6m, 12m, etc.
    public int? IsBasementFinished { get; set; } // 1 to filter
    public int? IsBasementUnfinished { get; set; } // 1 to filter
    public int? IsPendingUnderContract { get; set; } // 1 to filter
    public string? Keywords { get; set; } // Separated by comma
}