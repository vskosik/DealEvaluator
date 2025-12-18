using DealEvaluator.Application.DTOs.Lender;

namespace DealEvaluator.Application.Interfaces;

public interface ILenderService
{
    Task<List<LenderDto>> GetUserLendersAsync(string userId, bool includeArchived = false);
    Task<LenderDto?> GetUserLenderAsync(int lenderId, string userId);
    Task<LenderDto> CreateLenderAsync(string userId, CreateLenderDto dto);
    Task<LenderDto?> UpdateLenderAsync(string userId, UpdateLenderDto dto);
    Task DeleteLenderAsync(string userId, int lenderId);
}
