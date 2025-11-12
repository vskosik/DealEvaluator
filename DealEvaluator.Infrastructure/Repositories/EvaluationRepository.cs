using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Infrastructure.Repositories;

public class EvaluationRepository : DbRepository<Evaluation>, IEvaluationRepository
{
    private readonly DealEvaluatorContext _context;
    private readonly DbSet<Evaluation> _evaluations;

    public EvaluationRepository(DealEvaluatorContext context) : base(context)
    {
        _context = context;
        _evaluations = _context.Evaluations;
    }

    public async Task<List<Evaluation>> GetEvaluationsByPropertyIdAsync(int propertyId)
    {
        return await _evaluations
            .Include(e => e.Comparables)
            .Include(e => e.RehabEstimate)
                .ThenInclude(r => r.LineItems)
            .Where(e => e.PropertyId == propertyId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Evaluation?> GetLatestEvaluationByPropertyIdAsync(int propertyId)
    {
        return await _evaluations
            .Include(e => e.Comparables)
            .Include(e => e.RehabEstimate)
                .ThenInclude(r => r.LineItems)
            .Where(e => e.PropertyId == propertyId)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }
}