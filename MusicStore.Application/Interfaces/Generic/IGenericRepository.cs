using System.Linq.Expressions;

namespace MusicStore.Application.Interfaces.Generic
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);

        Task<IEnumerable<T>> GetAllAsync();

        Task<IEnumerable<T>> GetAllAsync(
            params Expression<Func<T, object>>[] includes);

        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate);

        Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null);
        Task<T?> GetByIdAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);


        Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null);

        Task<T?> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null);
    }
}