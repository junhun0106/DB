namespace RedisLib;

partial class RedisDatabase
{
    public Task<bool> SortedSetAddAsync<T>(string key, T value, double score, CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = this.Serializer.Serialize(value);

        return this.Database.SortedSetAddAsync(key, entryBytes, score, flag);
    }

    public Task<bool> SortedSetRemoveAsync<T>(
        string key,
        T value,
        CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = this.Serializer.Serialize(value);

        return this.Database.SortedSetRemoveAsync(key, entryBytes, flag);
    }

    public async Task<IEnumerable<T>> SortedSetRangeByScoreAsync<T>(
        string key,
        double start = double.NegativeInfinity,
        double stop = double.PositiveInfinity,
        Exclude exclude = Exclude.None,
        Order order = Order.Ascending,
        long skip = 0L,
        long take = -1L,
        CommandFlags flag = CommandFlags.None)
    {
        var result = await this.Database.SortedSetRangeByScoreAsync(key, start, stop, exclude, order, skip, take, flag).ConfigureAwait(false);

        return result.Select(m => m == RedisValue.Null ? default : this.Serializer.Deserialize<T>(m));
    }

    public async Task<IEnumerable<ScoreRankResult<T>>> SortedSetRangeByRankWithScoresAsync<T>(
        string key,
        long start = 0L,
        long stop = -1L,
        Order order = Order.Ascending,
        CommandFlags flag = CommandFlags.None)
    {
        var result = await this.Database.SortedSetRangeByRankWithScoresAsync(key, start, stop, order, flag).ConfigureAwait(false);

        return result
            .Select(x => new ScoreRankResult<T>(this.Serializer.Deserialize<T>(x.Element), x.Score))
            .ToArray();
    }
}
