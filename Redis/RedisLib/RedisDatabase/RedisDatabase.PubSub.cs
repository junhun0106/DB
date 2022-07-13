namespace RedisLib;

partial class RedisDatabase
{
    public Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flag = CommandFlags.None)
    {
        var sub = this.connectionPoolManager.GetConnection().GetSubscriber();
        return sub.PublishAsync(channel, this.Serializer.Serialize(message), flag);
    }
 
    public Task SubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flag = CommandFlags.None)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var sub = this.connectionPoolManager.GetConnection().GetSubscriber();

        async void Handler(RedisChannel redisChannel, RedisValue value) =>
            await handler(this.Serializer.Deserialize<T>(value))
                .ConfigureAwait(false);

        return sub.SubscribeAsync(channel, Handler, flag);
    }

    public Task UnsubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flag = CommandFlags.None)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var sub = this.connectionPoolManager.GetConnection().GetSubscriber();
        return sub.UnsubscribeAsync(channel, (_, value) => handler(this.Serializer.Deserialize<T>(value)), flag);
    }

    public Task UnsubscribeAllAsync(CommandFlags flag = CommandFlags.None)
    {
        var sub = this.connectionPoolManager.GetConnection().GetSubscriber();
        return sub.UnsubscribeAllAsync(flag);
    }

    public async Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
    {
        return await this.Database.KeyExistsAsync(key).ConfigureAwait(false)
            ? await this.Database.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow), flag).ConfigureAwait(false)
            : false;
    }

    public async Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
    {
        return await this.Database.KeyExistsAsync(key).ConfigureAwait(false)
            ? await this.Database.KeyExpireAsync(key, expiresIn, flag).ConfigureAwait(false)
            : false;
    }

    public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
    {
        var tasks = new Task<bool>[keys.Length];

        for (var i = 0; i < keys.Length; i++)
        {
            tasks[i] = this.UpdateExpiryAsync(keys[i], expiresAt.UtcDateTime, flag);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var results = new Dictionary<string, bool>(keys.Length, StringComparer.Ordinal);

        for (var i = 0; i < keys.Length; i++)
        {
            results.Add(keys[i], tasks[i].Result);
        }

        return results;
    }

    public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
    {
        var tasks = new Task<bool>[keys.Length];

        for (var i = 0; i < keys.Length; i++)
        {
            tasks[i] = this.UpdateExpiryAsync(keys[i], expiresIn, flag);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var results = new Dictionary<string, bool>(keys.Length, StringComparer.Ordinal);

        for (var i = 0; i < keys.Length; i++)
        {
            results.Add(keys[i], tasks[i].Result);
        }

        return results;
    }
}
