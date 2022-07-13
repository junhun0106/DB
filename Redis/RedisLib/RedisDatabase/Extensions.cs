namespace RedisLib;

using StackExchange.Redis;

internal static class Extensions
{
    public static TimeSpan GetExpireAt(this DateTime expire)
    {
        return ((DateTimeOffset)expire).UtcDateTime.Subtract(DateTime.UtcNow);
    }

    public static HashEntry[] GetEntries<T>(this IDictionary<string, T> dic, ISerializer serializer)
    {
        if (dic != null)
        {
            var entries = dic.Select(kv => new HashEntry(kv.Key, serializer.Serialize(kv.Value)));
            return entries.ToArray();
        }

        return null;
    }

    public static KeyValuePair<RedisKey, RedisValue>[] OfValueInListSize<T>(this ValueTuple<string, T>[] items, ISerializer serializer, uint maxValueLength)
    {
        var array = new KeyValuePair<RedisKey, RedisValue>[items.Length];

        for (int i = 0; i < items.Length; ++i)
        {
            var elem = items[i];
            var item1 = elem.Item1;
            var item2 = serializer.Serialize(elem.Item2).CheckLength(maxValueLength, item1);
            array[i] = new(item1, item2);
        }

        return array;
    }

    public static string OfValueSize<T>(this T value, ISerializer serializer, uint maxValueLength, string key)
    {
        return value == null
             ? string.Empty
             : serializer.Serialize(value).CheckLength(maxValueLength, key);
    }

    private static string CheckLength(this string value, uint maxValueLength, string paramName)
    {
        return maxValueLength > default(uint) && value.Length > maxValueLength
            ? throw new ArgumentException("value cannot be longer than the MaxValueLength", paramName)
            : value;
    }

    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
    {
        comparer ??= Comparer<TKey>.Default;

        using var sourceIterator = source.GetEnumerator();

        if (!sourceIterator.MoveNext())
        {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        var min = sourceIterator.Current;
        var minKey = selector(min);

        while (sourceIterator.MoveNext())
        {
            var candidate = sourceIterator.Current;
            var candidateProjected = selector(candidate);
            if (comparer.Compare(candidateProjected, minKey) < 0)
            {
                min = candidate;
                minKey = candidateProjected;
            }
        }

        return min;
    }
}
