using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Domain.Enums;
using DealEvaluator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Infrastructure.Repositories;

public class RehabCostTemplateRepository : DbRepository<RehabCostTemplate>, IRehabCostTemplateRepository
{
    private readonly DealEvaluatorContext _context;
    private readonly DbSet<RehabCostTemplate> _templates;

    public RehabCostTemplateRepository(DealEvaluatorContext context) : base(context)
    {
        _context = context;
        _templates = _context.RehabCostTemplates;
    }

    public async Task<List<RehabCostTemplate>> GetTemplatesByUserIdAsync(string userId)
    {
        return await _templates
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.LineItemType)
            .ThenBy(t => t.Condition)
            .ToListAsync();
    }

    public async Task<RehabCostTemplate?> GetTemplateByUserAndTypeConditionAsync(string userId, RehabLineItemType lineItemType, RehabCondition condition)
    {
        return await _templates
            .FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                t.LineItemType == lineItemType &&
                t.Condition == condition);
    }

    public async Task<List<RehabCostTemplate>> GetTemplatesByUserAndTypeAsync(string userId, RehabLineItemType lineItemType)
    {
        return await _templates
            .Where(t => t.UserId == userId && t.LineItemType == lineItemType)
            .OrderBy(t => t.Condition)
            .ToListAsync();
    }
}