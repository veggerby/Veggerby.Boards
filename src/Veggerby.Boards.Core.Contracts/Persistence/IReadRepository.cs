using System.Collections.Generic;
using System.Threading.Tasks;

namespace Veggerby.Boards.Core.Contracts.Persistence
{
    public interface IReadRepository<T>
    {
        Task<T> GetAsync(string id);
        Task<IEnumerable<T>> ListAsync();
    }
}