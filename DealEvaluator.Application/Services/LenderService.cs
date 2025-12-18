using AutoMapper;
using DealEvaluator.Application.DTOs.Lender;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Services;

public class LenderService : ILenderService
{
    private readonly ILenderRepository _repository;
    private readonly IMapper _mapper;

    public LenderService(ILenderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<LenderDto>> GetUserLendersAsync(string userId, bool includeArchived = false)
    {
        var lenders = await _repository.GetByUserIdAsync(userId, includeArchived);
        return _mapper.Map<List<LenderDto>>(lenders);
    }

    public async Task<LenderDto?> GetUserLenderAsync(int lenderId, string userId)
    {
        var lender = await _repository.GetByIdForUserAsync(lenderId, userId, includeArchived: true);
        return lender == null ? null : _mapper.Map<LenderDto>(lender);
    }

    public async Task<LenderDto> CreateLenderAsync(string userId, CreateLenderDto dto)
    {
        var lender = _mapper.Map<Lender>(dto);
        lender.UserId = userId;
        lender.CreatedAt = DateTime.UtcNow;
        lender.UpdatedAt = DateTime.UtcNow;

        await _repository.AddAsync(lender);
        await _repository.SaveChangesAsync();

        return _mapper.Map<LenderDto>(lender);
    }

    public async Task<LenderDto?> UpdateLenderAsync(string userId, UpdateLenderDto dto)
    {
        var lender = await _repository.GetByIdForUserAsync(dto.Id, userId);
        if (lender == null)
            return null;

        lender.Name = dto.Name;
        lender.AnnualRate = dto.AnnualRate;
        lender.OriginationFee = dto.OriginationFee;
        lender.LoanServiceFee = dto.LoanServiceFee;
        lender.Note = dto.Note;
        lender.UpdatedAt = DateTime.UtcNow;

        _repository.Update(lender);
        await _repository.SaveChangesAsync();

        return _mapper.Map<LenderDto>(lender);
    }

    public async Task DeleteLenderAsync(string userId, int lenderId)
    {
        var lender = await _repository.GetByIdForUserAsync(lenderId, userId, includeArchived: true);
        if (lender == null)
            return;

        lender.IsArchived = true;
        lender.ArchivedAt = DateTime.UtcNow;
        lender.UpdatedAt = DateTime.UtcNow;
        _repository.Update(lender);
        await _repository.SaveChangesAsync();
    }
}
