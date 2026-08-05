using Microsoft.EntityFrameworkCore.Storage;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Infrastructure.Data.Context;
using System.Collections;

namespace MusicStore.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MusicStoreDbContext _Context;

        private readonly Hashtable _repositories;

        private IDbContextTransaction? _transaction;


        public UnitOfWork(MusicStoreDbContext context)
        {
            _Context = context;

            _repositories = new Hashtable();
        }



        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T).Name;


            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new GenericRepository<T>(_Context);

                _repositories.Add(type, repositoryInstance);
            }


            return (IGenericRepository<T>)_repositories[type]!;
        }




        public async Task SaveAsync()
        {
            await _Context.SaveChangesAsync();
        }





        public async Task BeginTransactionAsync()
        {
            _transaction = await _Context.Database.BeginTransactionAsync();
        }





        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();

                await _transaction.DisposeAsync();

                _transaction = null;
            }
        }





        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();

                await _transaction.DisposeAsync();

                _transaction = null;
            }
        }
    }
}