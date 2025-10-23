using AutoMapper;
using DealEvaluator.Application.DTOs.Comparable;
using DealEvaluator.Application.DTOs.Evaluation;
using DealEvaluator.Application.DTOs.MarketData;
using DealEvaluator.Application.DTOs.Property;
using DealEvaluator.Application.DTOs.User;
using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Property mappings
        CreateMap<Property, PropertyDto>();
        CreateMap<CreatePropertyDto, Property>();
        CreateMap<UpdatePropertyDto, Property>();

        // Evaluation mappings
        CreateMap<Evaluation, EvaluationDto>();
        CreateMap<CreateEvaluationDto, Evaluation>();
        CreateMap<UpdateEvaluationDto, Evaluation>();

        // Comparable mappings
        CreateMap<Comparable, ComparableDto>();
        CreateMap<CreateComparableDto, Comparable>();

        // MarketData mappings
        CreateMap<MarketData, MarketDataDto>();
        CreateMap<CreateMarketDataDto, MarketData>();

        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<UpdateUserDto, User>();
    }
}