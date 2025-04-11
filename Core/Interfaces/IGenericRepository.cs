using System;
using Core.Entities;

namespace Core.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id); 
    Task<IReadOnlyList<T>> ListAllAsync();
    Task<T?> GetEntityWithSpec(ISpecification<T> spec);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
    
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> SaveChangesAsync();
    bool Exists(int id);
    Task<bool> SaveAllAsync();
}
