using System.Text.Json.Serialization;

namespace Deal_Evaluator.DTOs.Zillow;

public enum ZillowHomeType
{
    [JsonPropertyName("Multi-family")]
    MultiFamily,
    Apartments,
    Houses,
    Manufactured,
    Condos,
    LotsLand,
    Townhomes,
    [JsonPropertyName("Apartments_Condos_Co-ops")]
    ApartmentsCondosCoOps
}