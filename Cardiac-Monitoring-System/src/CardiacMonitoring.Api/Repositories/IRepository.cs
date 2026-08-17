namespace CardiacMonitoring.Api.Repositories;

// Generic repository contract shared by every entity in the system
// (Patient, VitalSign, Medication, Appointment) — one interface instead
// of writing a near-identical one for each entity type.
//
// "where T : class" is required because EF Core's DbSet<T> only works
// with reference types (our entities are all classes) — documenting this
// constraint here means a mismatched type fails at compile time with a
// clear message, instead of failing obscurely deeper in the code.
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> SaveChangesAsync();
}
