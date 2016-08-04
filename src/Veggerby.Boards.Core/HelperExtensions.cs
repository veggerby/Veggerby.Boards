using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core
{
    public static class HelperExtensions
    {
        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, T obj)
        {
            return list.Except(new [] { obj });
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> list, T obj)
        {
            return list.Concat(new [] { obj });
        }
    }
}