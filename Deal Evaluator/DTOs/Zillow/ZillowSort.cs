using System.Text.Json.Serialization;

namespace Deal_Evaluator.DTOs.Zillow;

public enum ZillowSort
{
    [JsonPropertyName("Homes_For_You")]
    HomesForYou,
    [JsonPropertyName("Price_High_Low")]
    PriceHighLow,
    [JsonPropertyName("Price_Low_High")]
    PriceLowHigh,
    Newest,
    Bedrooms,
    Bathrooms,
    [JsonPropertyName("Square_Feet")]
    SquareFeet,
    [JsonPropertyName("Lot_Size")]
    LotSize,
    [JsonPropertyName("Verified_Source")]
    VerifiedSource,
    [JsonPropertyName("Payment_High_Low")]
    PaymentHighLow,
    [JsonPropertyName("Payment_Low_High")]
    PaymentLowHigh
}