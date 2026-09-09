using Microsoft.EntityFrameworkCore;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Infrastructure.Data.Context;
using System.Collections;
using System.Linq.Expressions;

namespace MusicStore.Infrastructure.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly MusicStoreDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(MusicStoreDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }










        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }


        public async Task<T?> GetByIdAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }


        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }


        public async Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;


            if (predicate != null)
            {
                query = query.Where(predicate);
            }


            if (include != null)
            {
                query = include(query);
            }


            return await query.ToListAsync();
        }


        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }




        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }


        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }


        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }


        public async Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }


        public async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }
       
        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;


            if (include != null)
            {
                query = include(query);
            }


            return await query.FirstOrDefaultAsync(predicate);
        }

 
    }
}