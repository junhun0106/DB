namespace RedisLib
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StackExchange.Redis;

    public interface IRedisDatabase
    {
        IDatabase Database { get; }
        ISerializer Serializer { get; }

        Task<bool> AddAllAsync<T>(ValueTuple<string, T>[] items, When when = When.Always, CommandFlags flag = CommandFlags.None);
        Task<bool> AddAllAsync<T>(ValueTuple<string, T>[] items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);
        Task<bool> AddAllAsync<T>(ValueTuple<string, T>[] items, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None);
        Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None);
        Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);
        Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None);
        Task<bool> ExistsAsync(string key, CommandFlags flag = CommandFlags.None);
        Task FlushDbAsync();
        Task<IDictionary<string, T>> GetAllAsync<T>(string[] keys);
        Task<IDictionary<string, T>> GetAllAsync<T>(string[] keys, DateTimeOffset expiresAt);
        Task<IDictionary<string, T>> GetAllAsync<T>(string[] keys, TimeSpan expiresIn);
        Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None);
        Task<T> GetAsync<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);
        Task<T> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);
        Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags flag = CommandFlags.None);
        Task<long> HashDeleteAsync(string hashKey, string[] keys, CommandFlags flag = CommandFlags.None);
        Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags flag = CommandFlags.None);
        Task<Dictionary<string, T>> HashGetAllAsync<T>(string hashKey, CommandFlags flag = CommandFlags.None);
        Task<T> HashGetAsync<T>(string hashKey, string key, CommandFlags flag = CommandFlags.None);
        Task<Dictionary<string, T>> HashGetAsync<T>(string hashKey, string[] keys, CommandFlags flag = CommandFlags.None);
        Task<double> HashIncerementByAsync(string hashKey, string key, double value, CommandFlags flag = CommandFlags.None);
        Task<long> HashIncerementByAsync(string hashKey, string key, long value, CommandFlags flag = CommandFlags.None);
        Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags flag = CommandFlags.None);
        Task<long> HashLengthAsync(string hashKey, CommandFlags flag = CommandFlags.None);
        Task<Dictionary<string, T>> HashScanAsync<T>(string hashKey, string pattern, int pageSize = 10, long cursor = 0, int pageOffset = 0, CommandFlags flag = CommandFlags.None);
        Task HashSetAsync<T>(string hashKey, IDictionary<string, T> hashEntries, CommandFlags flag = CommandFlags.None);
        Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool notExists = false, CommandFlags flag = CommandFlags.None);
        Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CommandFlags flag = CommandFlags.None);
        Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flag = CommandFlags.None);
        Task<long> RemoveAllAsync(string[] keys, CommandFlags flag = CommandFlags.None);
        Task<bool> RemoveAsync(string key, CommandFlags flag = CommandFlags.None);
        Task<bool> ReplaceAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None);
        Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);
        Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None);
        Task SaveAsync(SaveType saveType, CommandFlags flag = CommandFlags.None);
        Task<IEnumerable<string>> SearchKeysAsync(string pattern);
        Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] values);
        Task<bool> SetAddAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None);
        Task<bool> SetContainsAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None);
        Task<string[]> SetMemberAsync(string key, CommandFlags flag = CommandFlags.None);
        Task<T[]> SetMembersAsync<T>(string key, CommandFlags flag = CommandFlags.None);
        Task<T> SetPopAsync<T>(string key, CommandFlags flag = CommandFlags.None);
        Task<IEnumerable<T>> SetPopAsync<T>(string key, long count, CommandFlags flag = CommandFlags.None);
        Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] values);
        Task<bool> SetRemoveAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None);
        Task<bool> SortedSetAddAsync<T>(string key, T value, double score, CommandFlags flag = CommandFlags.None);
        Task<double> SortedSetAddIncrementAsync<T>(string key, T value, double score, CommandFlags flag = CommandFlags.None);
        Task<IEnumerable<ScoreRankResult<T>>> SortedSetRangeByRankWithScoresAsync<T>(string key, long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flag = CommandFlags.None);
        Task<IEnumerable<T>> SortedSetRangeByScoreAsync<T>(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flag = CommandFlags.None);
        Task<bool> SortedSetRemoveAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None);
        Task SubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flag = CommandFlags.None);
        Task UnsubscribeAllAsync(CommandFlags flag = CommandFlags.None);
        Task UnsubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flag = CommandFlags.None);
        Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);
        Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);
        Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);
        Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);
    }
}
