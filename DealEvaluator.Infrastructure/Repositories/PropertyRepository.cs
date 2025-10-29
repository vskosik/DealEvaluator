using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Infrastructure.Repositories;

public class PropertyRepository : DbRepository<Property>, IPropertyRepository
{
    private readonly DealEvaluatorContext _context;
    private readonly DbSet<Property> _properties;
    private readonly DbSet<Comparable> _comparables;

    public PropertyRepository(DealEvaluatorContext context) : base(context)
    {
        _context = context;
        _properties = _context.Properties;
        _comparables = _context.Comparables;
    }

    // Get properties by user id
    public async Task<List<Property>> GetPropertiesByUserIdAsync(string userId)
    {
        return await _properties
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }
    
    // Get properties by zip code
    public async Task<List<Property>> GetPropertiesByZipCodeAsync(string zipCode)
    {
        return await _properties
            .Where(p => p.ZipCode == zipCode)
            .ToListAsync();
    }

    // Get properties in price range
    public async Task<List<Property>> GetPropertiesInPriceRangeAsync(decimal min, decimal max)
    {
        return await _properties
            .Where(p => p.Price >= min && p.Price <= max)
            .ToListAsync();
    }

    // Get comparables by property id
    public async Task<List<Comparable>> GetComparablesAsync(int propertyId)
    {
        return await _comparables
            .Where(c => c.PropertyId == propertyId)
            .ToListAsync();
    }

    // Get a single comparable by id
    public async Task<Comparable?> GetComparableByIdAsync(int comparableId)
    {
        return await _comparables
            .FirstOrDefaultAsync(c => c.Id == comparableId);
    }

    // Create a new comparable
    public async Task<Comparable> CreateComparableAsync(Comparable comparable)
    {
        await _comparables.AddAsync(comparable);
        await _context.SaveChangesAsync();
        return comparable;
    }

    // Delete a comparable
    public async Task DeleteComparableAsync(int comparableId)
    {
        var comparable = await GetComparableByIdAsync(comparableId);
        if (comparable != null)
        {
            _comparables.Remove(comparable);
            await _context.SaveChangesAsync();
        }
    }

    // Find property by address and user to prevent duplicates
    public async Task<Property?> GetPropertyByAddressAndUserAsync(string address, string userId)
    {
        return await _properties
            .FirstOrDefaultAsync(p =>
                p.Address == address &&
                p.UserId == userId);
    }
}