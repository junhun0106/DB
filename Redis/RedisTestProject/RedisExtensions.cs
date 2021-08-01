using JetBrains.Annotations;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisTestProject
{
    public static class RedisExtensions
    {
        public static TimeSpan GetExpireAt(this DateTime expire)
        {
            return ((DateTimeOffset)expire).UtcDateTime.Subtract(DateTime.UtcNow);
        }

        [CanBeNull]
        public static HashEntry[] GetEntries<T>(this IDictionary<string, T> dic, ISerializer serializer)
        {
            if (dic != null) {
                var entries = dic.Select(kv => new HashEntry(kv.Key, serializer.Serialize(kv.Value)));
                return entries.ToArray();
            }

            return null;
        }
    }
}
