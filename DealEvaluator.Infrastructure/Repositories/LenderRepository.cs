using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Infrastructure.Repositories;

public class LenderRepository : DbRepository<Lender>, ILenderRepository
{
    private readonly DealEvaluatorContext _context;
    private readonly DbSet<Lender> _lenders;

    public LenderRepository(DealEvaluatorContext context) : base(context)
    {
        _context = context;
        _lenders = context.Lenders;
    }

    public async Task<List<Lender>> GetByUserIdAsync(string userId, bool includeArchived = false)
    {
        return await _lenders
            .Where(l => l.UserId == userId && (includeArchived || !l.IsArchived))
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<Lender?> GetByIdForUserAsync(int lenderId, string userId, bool includeArchived = false)
    {
        return await _lenders
            .Where(l => l.UserId == userId && l.Id == lenderId && (includeArchived || !l.IsArchived))
            .FirstOrDefaultAsync();
    }
}
