namespace RedisLib;

partial class RedisDatabase
{
    public Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags flag = CommandFlags.None)
    {
        return this.Database.HashDeleteAsync(hashKey, key, flag);
    }

    public Task<long> HashDeleteAsync(string hashKey, string[] keys, CommandFlags flag = CommandFlags.None)
    {
        var redisKeys = new RedisValue[keys.Length];

        for (var i = 0; i < keys.Length; i++)
        {
            redisKeys[i] = (RedisValue)keys[i];
        }

        return this.Database.HashDeleteAsync(hashKey, redisKeys, flag);
    }

    public Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags flag = CommandFlags.None)
    {
        return this.Database.HashExistsAsync(hashKey, key, flag);
    }

    public async Task<T> HashGetAsync<T>(string hashKey, string key, CommandFlags flag = CommandFlags.None)
    {
        var redisValue = await this.Database.HashGetAsync(hashKey, key, flag).ConfigureAwait(false);

        return redisValue.HasValue ? this.Serializer.Deserialize<T>(redisValue) : default;
    }

    public async Task<Dictionary<string, T>> HashGetAsync<T>(string hashKey, string[] keys, CommandFlags flag = CommandFlags.None)
    {
        var tasks = new Task<T>[keys.Length];

        for (var i = 0; i < keys.Length; i++)
        {
            tasks[i] = this.HashGetAsync<T>(hashKey, keys[i], flag);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var result = new Dictionary<string, T>();

        for (var i = 0; i < tasks.Length; i++)
        {
            result.Add(keys[i], tasks[i].Result);
        }

        return result;
    }

    public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string hashKey, CommandFlags flag = CommandFlags.None)
    {
        return (await this.Database.HashGetAllAsync(hashKey, flag).ConfigureAwait(false))
            .ToDictionary(
                x => x.Name.ToString(),
                x => this.Serializer.Deserialize<T>(x.Value),
                StringComparer.Ordinal);
    }

    public Task<long> HashIncerementByAsync(string hashKey, string key, long value, CommandFlags flag = CommandFlags.None)
    {
        return this.Database.HashIncrementAsync(hashKey, key, value, flag);
    }

    public Task<double> HashIncerementByAsync(string hashKey, string key, double value, CommandFlags flag = CommandFlags.None)
    {
        return this.Database.HashIncrementAsync(hashKey, key, value, flag);
    }

    public async Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags flag = CommandFlags.None)
    {
        return (await this.Database.HashKeysAsync(hashKey, flag).ConfigureAwait(false)).Select(x => x.ToString());
    }

    public Task<long> HashLengthAsync(string hashKey, CommandFlags flag = CommandFlags.None)
    {
        return this.Database.HashLengthAsync(hashKey, flag);
    }

    public Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool notExists = false, CommandFlags flag = CommandFlags.None)
    {
        return this.Database.HashSetAsync(hashKey, key, this.Serializer.Serialize(value), notExists ? When.NotExists : When.Always, flag);
    }

    public Task HashSetAsync<T>(string hashKey, IDictionary<string, T> hashEntries, CommandFlags flag = CommandFlags.None)
    {
        var entries = hashEntries.Select(kv => new HashEntry(kv.Key, this.Serializer.Serialize(kv.Value)));

        return this.Database.HashSetAsync(hashKey, entries.ToArray(), flag);
    }

    public async Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CommandFlags flag = CommandFlags.None)
    {
        return (await this.Database.HashValuesAsync(hashKey, flag).ConfigureAwait(false)).Select(x => this.Serializer.Deserialize<T>(x));
    }

    public async Task<Dictionary<string, T>> HashScanAsync<T>(string hashKey, string pattern, int pageSize = 10, long cursor = 0, int pageOffset = 0, CommandFlags flag = CommandFlags.None)
    {
        var asyncEnumerable = this.Database.HashScanAsync(hashKey, pattern, pageSize, cursor, pageOffset, flag);
        var dic = new Dictionary<string, T>(StringComparer.Ordinal);
        await foreach (var hashEntry in asyncEnumerable)
        {
            dic.Add(hashEntry.Name.ToString(), this.Serializer.Deserialize<T>(hashEntry.Value));
        }
        return dic;
    }
}
