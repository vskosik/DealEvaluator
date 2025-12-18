using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Interfaces;

public interface ILenderRepository : IRepository<Lender>
{
    Task<List<Lender>> GetByUserIdAsync(string userId, bool includeArchived = false);
    Task<Lender?> GetByIdForUserAsync(int lenderId, string userId, bool includeArchived = false);
}
