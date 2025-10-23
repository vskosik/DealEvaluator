using DealEvaluator.Application.Interfaces;
using DealEvaluator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Infrastructure.Repositories;

public class DbRepository<T> : IRepository<T> where T : class
{
    private readonly DealEvaluatorContext _context;
    private readonly DbSet<T> _dbSet;
    
    public DbRepository(DealEvaluatorContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }
    
    public async Task AddAsync(T model)
    {
        await _dbSet.AddAsync(model);
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}