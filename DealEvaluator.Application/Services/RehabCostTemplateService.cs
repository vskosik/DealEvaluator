using AutoMapper;
using DealEvaluator.Application.DTOs.Rehab;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.Services;

public class RehabCostTemplateService : IRehabCostTemplateService
{
    private readonly IRehabCostTemplateRepository _templateRepository;
    private readonly IMapper _mapper;

    public RehabCostTemplateService(
        IRehabCostTemplateRepository templateRepository,
        IMapper mapper)
    {
        _templateRepository = templateRepository;
        _mapper = mapper;
    }

    public async Task<List<RehabCostTemplateDto>> GetUserTemplatesAsync(string userId)
    {
        var templates = await _templateRepository.GetTemplatesByUserIdAsync(userId);
        return _mapper.Map<List<RehabCostTemplateDto>>(templates);
    }

    public async Task<RehabCostTemplateDto?> GetTemplateAsync(string userId, RehabLineItemType lineItemType, RehabCondition condition)
    {
        var template = await _templateRepository.GetTemplateByUserAndTypeConditionAsync(userId, lineItemType, condition);
        return _mapper.Map<RehabCostTemplateDto?>(template);
    }

    public async Task<RehabCostTemplateDto> UpsertTemplateAsync(string userId, RehabLineItemType lineItemType, RehabCondition condition, decimal defaultCost)
    {
        // Check if template already exists
        var existingTemplate = await _templateRepository.GetTemplateByUserAndTypeConditionAsync(userId, lineItemType, condition);

        if (existingTemplate != null)
        {
            // Update existing template
            existingTemplate.DefaultCost = defaultCost;
            existingTemplate.UpdatedAt = DateTime.UtcNow;

            _templateRepository.Update(existingTemplate);
            await _templateRepository.SaveChangesAsync();

            return _mapper.Map<RehabCostTemplateDto>(existingTemplate);
        }

        // Create new template
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

            // Other
            (RehabLineItemType.Other, RehabCondition.Cosmetic, 1000m),
            (RehabLineItemType.Other, RehabCondition.Moderate, 3000m),
            (RehabLineItemType.Other, RehabCondition.Heavy, 8000m),

            // General (per square foot)
            (RehabLineItemType.General, RehabCondition.Cosmetic, 10m),
            (RehabLineItemType.General, RehabCondition.Moderate, 20m),
            (RehabLineItemType.General, RehabCondition.Heavy, 35m),
        };
    }
}