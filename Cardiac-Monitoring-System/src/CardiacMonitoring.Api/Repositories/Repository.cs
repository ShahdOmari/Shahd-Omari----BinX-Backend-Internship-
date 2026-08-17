using CardiacMonitoring.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitoring.Api.Repositories;

// Single generic implementation reused for every entity — avoids writing
// four nearly-identical repository classes (one per entity) that would
// only differ in their type parameter.
public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    // Returns IReadOnlyList, not List — callers can read the result but
    // never mutate it directly (Week 2's "return the least permissive
    // interface that still satisfies the caller" rule).
    public async Task<IReadOnlyList<T>> GetAllAsync() =>
        await _dbSet.AsNoTracking().ToListAsync();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);

    public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
}
