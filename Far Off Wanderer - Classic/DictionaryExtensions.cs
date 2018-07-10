using System;
using System.Collections.Generic;
using System.Linq;

namespace Far_Off_Wanderer
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TOut> SelectAsDictionary<TKey, TIn, TOut>(this Dictionary<TKey, TIn> dictionary, Func<TIn, TOut> select)
        {
            return dictionary.Select(entry => new
            {
                entry.Key,
                Value = select(entry.Value)
            }).ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        public static Dictionary<TKey, TOut> SelectAsDictionary<TKey, TOut>(this IEnumerable<TKey> keys, Func<TKey, TOut> select)
        {
            return keys.Where(key => key != null).Distinct().Select(key => new
            {
                Key = key,
                Value = select(key)
            }).ToDictionary(entry => entry.Key, entry => entry.Value);
        }
    }
}
