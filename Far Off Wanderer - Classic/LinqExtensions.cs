using System.Collections.Generic;
using System.Linq;

namespace Far_Off_Wanderer
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> Except<T>(this IEnumerable<T> items, T item) => items.Except(new T[] { item });
    }
}
