using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.Application
{
    internal static class Extensions
    {
        public static Dictionary<TKey, TElement> ToDictionaryIgnoringDuplicates<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector)
        {
            Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>();
            foreach (var item in source)
            {
                var key = keySelector(item);
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, elementSelector(item));
                }
            }
            return dictionary;
        }
    }
}
