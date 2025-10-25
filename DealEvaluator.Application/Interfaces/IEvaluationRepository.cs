using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Interfaces;

public interface IEvaluationRepository : IRepository<Evaluation>
{
    Task<List<Evaluation>> GetEvaluationsByPropertyIdAsync(int propertyId);
    Task<Evaluation?> GetLatestEvaluationByPropertyIdAsync(int propertyId);
}