using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Veggerby.Boards.Core.Contracts.Persistence
{
    public interface IRepository<T>
    {
        Task<T> GetAsync(string id);
        Task<IEnumerable<T>> ListAsync();
        Task<IEnumerable<T>> ListAsync(Expression<Predicate<T>> query);
        Task<bool> InsertAsync(T obj);
        Task<bool> ReplaceAsync(T obj);
        Task<bool> InsertOrReplaceAsync(T obj);
        Task<bool> DeleteAsync(T obj);
    }
}
