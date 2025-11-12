using AutoMapper;
using DealEvaluator.Application.DTOs.Rehab;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Application.Services;

public class RehabCostTemplateService : IRehabCostTemplateService
{
    private readonly IRepository<RehabCostTemplate> _templateRepository;
    private readonly IMapper _mapper;

    public RehabCostTemplateService(
        IRepository<RehabCostTemplate> templateRepository,
        IMapper mapper)
    {
        _templateRepository = templateRepository;
        _mapper = mapper;
    }

    public async Task<List<RehabCostTemplateDto>> GetUserTemplatesAsync(string userId)
    {
        // Note: This requires a custom query beyond the basic IRepository interface
        // For now, returning empty list - will need to add custom repository method
        // TODO: Add GetByUserIdAsync to IRepository or create specific interface
        return new List<RehabCostTemplateDto>();
    }

    public async Task<RehabCostTemplateDto?> GetTemplateAsync(string userId, RehabLineItemType lineItemType, RehabCondition condition)
    {
        // Note: This requires a custom query beyond the basic IRepository interface
        // TODO: Add custom query method to repository
        return null;
    }

    public async Task<RehabCostTemplateDto> UpsertTemplateAsync(string userId, RehabLineItemType lineItemType, RehabCondition condition, decimal defaultCost)
    {
        // Note: This requires a custom query to find existing template
        // For now, creating a basic implementation
        var template = new RehabCostTemplate
        {
            UserId = userId,
            LineItemType = lineItemType,
            Condition = condition,
            DefaultCost = defaultCost,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _templateRepository.AddAsync(template);
        await _templateRepository.SaveChangesAsync();

        return _mapper.Map<RehabCostTemplateDto>(template);
    }

    public async Task DeleteTemplateAsync(int templateId)
    {
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template != null)
        {
            _templateRepository.Delete(template);
            await _templateRepository.SaveChangesAsync();
        }
    }

    public async Task SeedDefaultTemplatesAsync(string userId)
    {
        var defaultTemplates = GetDefaultTemplateCosts();

        foreach (var (lineItemType, condition, cost) in defaultTemplates)
        {
            var template = new RehabCostTemplate
            {
                UserId = userId,
                LineItemType = lineItemType,
                Condition = condition,
                DefaultCost = cost,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _templateRepository.AddAsync(template);
        }

        await _templateRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Returns default cost templates for each line item type and condition
    /// Costs are rough estimates and can be customized by users
    /// </summary>
    private static List<(RehabLineItemType LineItemType, RehabCondition Condition, decimal Cost)> GetDefaultTemplateCosts()
    {
        return new List<(RehabLineItemType, RehabCondition, decimal)>
        {
            // Kitchen costs
            (RehabLineItemType.Kitchen, RehabCondition.Cosmetic, 5000m),
            (RehabLineItemType.Kitchen, RehabCondition.Moderate, 15000m),
            (RehabLineItemType.Kitchen, RehabCondition.Heavy, 30000m),

            // Bathroom costs
            (RehabLineItemType.Bathroom, RehabCondition.Cosmetic, 3000m),
            (RehabLineItemType.Bathroom, RehabCondition.Moderate, 8000m),
            (RehabLineItemType.Bathroom, RehabCondition.Heavy, 15000m),

            // Bedroom costs
            (RehabLineItemType.MasterBedroom, RehabCondition.Cosmetic, 2000m),
            (RehabLineItemType.MasterBedroom, RehabCondition.Moderate, 5000m),
            (RehabLineItemType.MasterBedroom, RehabCondition.Heavy, 10000m),

            (RehabLineItemType.Bedroom, RehabCondition.Cosmetic, 1500m),
            (RehabLineItemType.Bedroom, RehabCondition.Moderate, 3500m),
            (RehabLineItemType.Bedroom, RehabCondition.Heavy, 7000m),

            // Living areas
            (RehabLineItemType.LivingRoom, RehabCondition.Cosmetic, 2000m),
            (RehabLineItemType.LivingRoom, RehabCondition.Moderate, 5000m),
            (RehabLineItemType.LivingRoom, RehabCondition.Heavy, 10000m),

            (RehabLineItemType.DiningRoom, RehabCondition.Cosmetic, 1500m),
            (RehabLineItemType.DiningRoom, RehabCondition.Moderate, 3500m),
            (RehabLineItemType.DiningRoom, RehabCondition.Heavy, 7000m),

            // Basement
            (RehabLineItemType.Basement, RehabCondition.Cosmetic, 3000m),
            (RehabLineItemType.Basement, RehabCondition.Moderate, 10000m),
            (RehabLineItemType.Basement, RehabCondition.Heavy, 25000m),

            // Exterior and structural
            (RehabLineItemType.Exterior, RehabCondition.Cosmetic, 3000m),
            (RehabLineItemType.Exterior, RehabCondition.Moderate, 10000m),
            (RehabLineItemType.Exterior, RehabCondition.Heavy, 25000m),

            (RehabLineItemType.Roof, RehabCondition.Cosmetic, 2000m),
            (RehabLineItemType.Roof, RehabCondition.Moderate, 8000m),
            (RehabLineItemType.Roof, RehabCondition.Heavy, 15000m),

            // Systems
            (RehabLineItemType.HVAC, RehabCondition.Cosmetic, 1000m),
            (RehabLineItemType.HVAC, RehabCondition.Moderate, 5000m),
            (RehabLineItemType.HVAC, RehabCondition.Heavy, 12000m),

            (RehabLineItemType.Plumbing, RehabCondition.Cosmetic, 1000m),
            (RehabLineItemType.Plumbing, RehabCondition.Moderate, 5000m),
            (RehabLineItemType.Plumbing, RehabCondition.Heavy, 15000m),

            (RehabLineItemType.Electrical, RehabCondition.Cosmetic, 1000m),
            (RehabLineItemType.Electrical, RehabCondition.Moderate, 5000m),
            (RehabLineItemType.Electrical, RehabCondition.Heavy, 12000m),

            // Finishes
            (RehabLineItemType.Flooring, RehabCondition.Cosmetic, 2000m),
            (RehabLineItemType.Flooring, RehabCondition.Moderate, 6000m),
            (RehabLineItemType.Flooring, RehabCondition.Heavy, 12000m),

            (RehabLineItemType.Windows, RehabCondition.Cosmetic, 1500m),
            (RehabLineItemType.Windows, RehabCondition.Moderate, 5000m),
            (RehabLineItemType.Windows, RehabCondition.Heavy, 12000m),

            (RehabLineItemType.Doors, RehabCondition.Cosmetic, 1000m),
            (RehabLineItemType.Doors, RehabCondition.Moderate, 3000m),
            (RehabLineItemType.Doors, RehabCondition.Heavy, 7000m),

            (RehabLineItemType.Landscaping, RehabCondition.Cosmetic, 1000m),
            (RehabLineItemType.Landscaping, RehabCondition.Moderate, 3000m),
            (RehabLineItemType.Landscaping, RehabCondition.Heavy, 8000m),

            // Other
            (RehabLineItemType.Other, RehabCondition.Cosmetic, 1000m),
            (RehabLineItemType.Other, RehabCondition.Moderate, 3000m),
            (RehabLineItemType.Other, RehabCondition.Heavy, 8000m),
        };
    }
}