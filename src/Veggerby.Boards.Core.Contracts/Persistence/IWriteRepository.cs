using System.Threading.Tasks;

namespace Veggerby.Boards.Core.Contracts.Persistence
{
    public interface IWriteRepository<T>
    {
        Task<bool> InsertAsync(T obj);
        Task<bool> ReplaceAsync(T obj);
        Task<bool> InsertOrReplaceAsync(T obj);
        Task<bool> DeleteAsync(T obj);
    }
}
