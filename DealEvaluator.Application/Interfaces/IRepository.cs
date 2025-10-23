namespace DealEvaluator.Application.Interfaces;

public interface IRepository<T> where T : class
{
    // Create
    Task AddAsync(T model);

    // Read
    Task<T?> GetByIdAsync(int id);

    // Update
    void Update(T entity);

    // Delete
    void Delete(T entity);

    // Save Changes
    Task<int> SaveChangesAsync();
}