using System.Collections.Concurrent;

namespace Migrator.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
        {
            var dictionary = new ConcurrentDictionary<TKey, TValue>();
            foreach (var item in source)
            {
                dictionary.TryAdd(keySelector(item), valueSelector(item));
            }
            return dictionary;
        }

        public static ConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TSource, TKey>(this IEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            return source.ToConcurrentDictionary(keySelector, item => item);
        }


        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, Func<TValue, TValue, TValue> updateValueFactory)
        {
            var dictionary = new ConcurrentDictionary<TKey, TValue>();
            foreach (var item in source)
            {
                dictionary.AddOrUpdate(
                    keySelector(item),
                    valueSelector(item),
                    (key, existingVal) => updateValueFactory(existingVal, valueSelector(item)));
            }
            return dictionary;
        }
    }
}
