using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Infrastructure.Repositories;

public class DealSettingsRepository : DbRepository<DealSettings>, IDealSettingsRepository
{
    private readonly DealEvaluatorContext _context;

    public DealSettingsRepository(DealEvaluatorContext context) : base(context)
    {
        _context = context;
    }

    public async Task<DealSettings?> GetByUserIdAsync(string userId)
    {
        return await _context.DealSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }
}