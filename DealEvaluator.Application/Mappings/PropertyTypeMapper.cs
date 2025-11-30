using DealEvaluator.Application.DTOs.Zillow;
using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.Mappings;

public static class PropertyTypeMapper
{
    public static ZillowHomeType ToZillowHomeType(PropertyTypes propertyType)
    {
        return propertyType switch
        {
            PropertyTypes.SingleFamily => ZillowHomeType.Houses,
            PropertyTypes.MultiFamily => ZillowHomeType.MultiFamily,
            PropertyTypes.Condo => ZillowHomeType.Condos,
            PropertyTypes.Townhouse => ZillowHomeType.Townhomes,
            _ => throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, "Unsupported property type")
        };
    }

    public static string ToZillowHomeTypeString(PropertyTypes propertyType)
    {
        return ToZillowHomeType(propertyType).ToString();
    }
}