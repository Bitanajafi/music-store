using MusicStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.Interfaces.Generic
{
    public interface IUnitOfWork
    {
        IGenericRepository<T> Repository<T>() where T : class;

        Task SaveAsync();


        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}
