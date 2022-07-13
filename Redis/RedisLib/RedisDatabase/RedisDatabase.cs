namespace RedisLib;

using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

partial class RedisDatabase : IRedisDatabase
{
    private readonly RedisConnectionPoolManager connectionPoolManager;
    private readonly ServerEnumerationStrategy serverEnumerationStrategy;
    private readonly string keyPrefix;
    private readonly uint maxValueLength;
    private readonly int dbNumber;

    public RedisDatabase(
        RedisConnectionPoolManager connectionPoolManager,
        ISerializer serializer,
        ServerEnumerationStrategy serverEnumerationStrategy,
        int dbNumber,
        uint maxvalueLength,
        string keyPrefix = null)
    {
        this.Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        this.connectionPoolManager = connectionPoolManager ?? throw new ArgumentNullException(nameof(connectionPoolManager));
        this.serverEnumerationStrategy = serverEnumerationStrategy;
        this.dbNumber = dbNumber;
        this.keyPrefix = keyPrefix ?? string.Empty;
        this.maxValueLength = maxvalueLength;
    }

    public IDatabase Database
    {
        get
        {
            var db = this.connectionPoolManager.GetConnection().GetDatabase(this.dbNumber);

            return this.keyPrefix.Length > 0
                ? db.WithKeyPrefix(this.keyPrefix)
                : db;
        }
    }

    public ISerializer Serializer { get; }

    public Task<bool> ExistsAsync(string key, CommandFlags flag = CommandFlags.None)
    {
        return this.Database.KeyExistsAsync(key, flag);
    }

    public Task<bool> RemoveAsync(string key, CommandFlags flag = CommandFlags.None)
    {
        return this.Database.KeyDeleteAsync(key, flag);
    }

    public Task<long> RemoveAllAsync(string[] keys, CommandFlags flag = CommandFlags.None)
    {
        var redisKeys = new RedisKey[keys.Length];

        for (var i = 0; i < keys.Length; i++)
        {
            redisKeys[i] = (RedisKey)keys[i];
        }

        return this.Database.KeyDeleteAsync(redisKeys, flag);
    }

    public async Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
    {
        var valueBytes = await this.Database
            .StringGetAsync(key, flag)
            .ConfigureAwait(false);

        return !valueBytes.HasValue
            ? default
            : this.Serializer.Deserialize<T>(valueBytes);
    }

    public async Task<T> GetAsync<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
    {
        var result = await this.GetAsync<T>(key, flag).ConfigureAwait(false);

        if (!EqualityComparer<T>.Default.Equals(result, default))
        {
            await this.Database.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow)).ConfigureAwait(false);
        }

        return result;
    }

    public async Task<T> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
    {
        var result = await this.GetAsync<T>(key, flag).ConfigureAwait(false);

        if (!EqualityComparer<T>.Default.Equals(result, default))
        {
            await this.Database.KeyExpireAsync(key, expiresIn).ConfigureAwait(false);
        }

        return result;
    }

    public Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = value.OfValueSize(this.Serializer, this.maxValueLength, key);
        return this.Database.StringSetAsync(key, entryBytes, null, when, flag);
    }

    public Task<bool> ReplaceAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        return this.AddAsync(key, value, when, flag);
    }

    public Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = value.OfValueSize(this.Serializer, this.maxValueLength, key);

        var expiration = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

        return this.Database.StringSetAsync(key, entryBytes, expiration, when, flag);
    }

    public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        return this.AddAsync(key, value, expiresAt, when, flag);
    }

    public Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = value.OfValueSize(this.Serializer, this.maxValueLength, key);
        return this.Database.StringSetAsync(key, entryBytes, expiresIn, when, flag);
    }

    public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        return this.AddAsync(key, value, expiresIn, when, flag);
    }

    public async Task<IDictionary<string, T>> GetAllAsync<T>(string[] keys)
    {
        var redisKeys = new RedisKey[keys.Length];

        for (var i = 0; i < keys.Length; i++)
        {
            redisKeys[i] = (RedisKey)keys[i];
        }

        var result = await this.Database.StringGetAsync(redisKeys).ConfigureAwait(false);

        var dict = new Dictionary<string, T>(redisKeys.Length, StringComparer.Ordinal);

        for (var index = 0; index < redisKeys.Length; index++)
        {
            var value = result[index];
            dict.Add(redisKeys[index], value == RedisValue.Null
                ? default
                : this.Serializer.Deserialize<T>(value));
        }

        return dict;
    }

    public async Task<IDictionary<string, T>> GetAllAsync<T>(string[] keys, DateTimeOffset expiresAt)
    {
        var tsk1 = this.GetAllAsync<T>(keys);
        var tsk2 = this.UpdateExpiryAllAsync(keys, expiresAt);

        await Task.WhenAll(tsk1, tsk2).ConfigureAwait(false);

        return tsk1.Result;
    }

    public async Task<IDictionary<string, T>> GetAllAsync<T>(string[] keys, TimeSpan expiresIn)
    {
        var tsk1 = this.GetAllAsync<T>(keys);
        var tsk2 = this.UpdateExpiryAllAsync(keys, expiresIn);

        await Task.WhenAll(tsk1, tsk2).ConfigureAwait(false);

        return tsk1.Result;
    }

    public Task<bool> AddAllAsync<T>(ValueTuple<string, T>[] items, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var values = items.OfValueInListSize(this.Serializer, this.maxValueLength);
        return this.Database.StringSetAsync(values, when, flag);
    }

    public async Task<bool> AddAllAsync<T>(ValueTuple<string, T>[] items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var values = items.OfValueInListSize(this.Serializer, this.maxValueLength);
        var tasks = new Task[values.Length];
        await this.Database.StringSetAsync(values, when, flag).ConfigureAwait(false);

        for (var i = 0; i < values.Length; i++)
        {
            tasks[i] = this.Database.KeyExpireAsync(values[i].Key, expiresAt.UtcDateTime, flag);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return ((Task<bool>)tasks[0]).Result;
    }

    public async Task<bool> AddAllAsync<T>(ValueTuple<string, T>[] items, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var values = items.OfValueInListSize(this.Serializer, this.maxValueLength);
        var tasks = new Task[values.Length];
        await this.Database.StringSetAsync(values, when, flag).ConfigureAwait(false);

        for (var i = 0; i < values.Length; i++)
        {
            tasks[i] = this.Database.KeyExpireAsync(values[i].Key, expiresIn, flag);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return ((Task<bool>)tasks[0]).Result;
    }

    public Task<bool> SetAddAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("key cannot be empty.", nameof(key));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "item cannot be null.");
        }

        var serializedObject = this.Serializer.Serialize(value);

        return this.Database.SetAddAsync(key, serializedObject, flag);
    }

    public async Task<T> SetPopAsync<T>(string key, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("key cannot be empty.", nameof(key));
        }

        var item = await this.Database.SetPopAsync(key, flag).ConfigureAwait(false);

        return item == RedisValue.Null
            ? default
            : this.Serializer.Deserialize<T>(item);
    }

    public async Task<IEnumerable<T>> SetPopAsync<T>(string key, long count, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("key cannot be empty.", nameof(key));
        }

        var items = await this.Database.SetPopAsync(key, count, flag).ConfigureAwait(false);

        return items.Select(item => item == RedisValue.Null ? default : this.Serializer.Deserialize<T>(item));
    }

    public Task<bool> SetContainsAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("key cannot be empty.", nameof(key));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "item cannot be null.");
        }

        var serializedObject = this.Serializer.Serialize(value);

        return this.Database.SetContainsAsync(key, serializedObject, flag);
    }

    public Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] values)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("key cannot be empty.", nameof(key));
        }

        if (values == null)
        {
            throw new ArgumentNullException(nameof(values), "items cannot be null.");
        }

        return values.Any(item => item == null)
            ? throw new ArgumentException("items cannot contains any null item.", nameof(values))
            : this.Database
            .SetAddAsync(
                key,
                values
                    .Select(item => this.Serializer.Serialize(item))
                    .Select(x => (RedisValue)x)
                    .ToArray(),
                flag);
    }

    public Task<bool> SetRemoveAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("key cannot be empty.", nameof(key));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "item cannot be null.");
        }

        var serializedObject = this.Serializer.Serialize(value);

        return this.Database.SetRemoveAsync(key, serializedObject, flag);
    }

    public Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] values)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("key cannot be empty.", nameof(key));
        }

        if (values == null)
        {
            throw new ArgumentNullException(nameof(values), "items cannot be null.");
        }

        return values.Any(item => item == null)
            ? throw new ArgumentException("items cannot contains any null item.", nameof(values))
            : this.Database.SetRemoveAsync(key, values
            .Select(item => this.Serializer.Serialize(item))
            .Select(x => (RedisValue)x)
            .ToArray(), flag);
    }

    public async Task<string[]> SetMemberAsync(string key, CommandFlags flag = CommandFlags.None)
    {
        var members = await this.Database.SetMembersAsync(key, flag).ConfigureAwait(false);
        return members.Select(x => x.ToString()).ToArray();
    }

    public async Task<T[]> SetMembersAsync<T>(string key, CommandFlags flag = CommandFlags.None)
    {
        var members = await this.Database.SetMembersAsync(key, flag).ConfigureAwait(false);

        if (members.Length == 0)
        {
            return Array.Empty<T>();
        }

        var result = new T[members.Length];

        for (var i = 0; i < members.Length; i++)
        {
            var m = members[i];

            result[i] = this.Serializer.Deserialize<T>(m);
        }

        return result;
    }

    public async Task<IEnumerable<string>> SearchKeysAsync(string pattern)
    {
        pattern = $"{this.keyPrefix}{pattern}";
        var keys = new HashSet<string>();

        var servers = ServerIteratorFactory
            .GetServers(this.connectionPoolManager.GetConnection(), this.serverEnumerationStrategy)
            .ToArray();

        if (servers.Length == 0)
        {
            throw new("No server found to serve the KEYS command.");
        }

        foreach (var unused in servers)
        {
            long nextCursor = 0;
            do
            {
                var redisResult = await this.Database.ExecuteAsync("SCAN", nextCursor.ToString(), "MATCH", pattern, "COUNT", "1000").ConfigureAwait(false);
                var innerResult = (RedisResult[])redisResult;

                nextCursor = long.Parse((string)innerResult[0]);

                var resultLines = ((string[])innerResult[1]).ToArray();
                keys.UnionWith(resultLines);
            }
            while (nextCursor != 0);
        }

        return !string.IsNullOrEmpty(this.keyPrefix)
            ? keys.Select(k => k.Substring(this.keyPrefix.Length))
            : keys;
    }

    public Task FlushDbAsync()
    {
        var endPoints = this.Database.Multiplexer.GetEndPoints();

        var tasks = new List<Task>(endPoints.Length);

        for (var i = 0; i < endPoints.Length; i++)
        {
            var server = this.Database.Multiplexer.GetServer(endPoints[i]);

            if (!server.IsReplica)
            {
                tasks.Add(server.FlushDatabaseAsync(this.Database.Database));
            }
        }

        return Task.WhenAll(tasks);
    }

    public Task SaveAsync(SaveType saveType, CommandFlags flag = CommandFlags.None)
    {
        var endPoints = this.Database.Multiplexer.GetEndPoints();

        var tasks = new Task[endPoints.Length];

        for (var i = 0; i < endPoints.Length; i++)
        {
            tasks[i] = this.Database.Multiplexer.GetServer(endPoints[i]).SaveAsync(saveType, flag);
        }

        return Task.WhenAll(tasks);
    }

    public Task<double> SortedSetAddIncrementAsync<T>(string key, T value, double score, CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = this.Serializer.Serialize(value);
        return this.Database.SortedSetIncrementAsync(key, entryBytes, score, flag);
    }
}
