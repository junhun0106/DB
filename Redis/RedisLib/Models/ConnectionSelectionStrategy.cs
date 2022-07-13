namespace RedisLib;

enum ConnectionSelectionStrategy
{
    RoundRobin,

    /// <summary>
    /// Every call will return the least loaded <see cref="IConnectionMultiplexer"/>.
    /// The load of every connection is defined by it's <see cref="ServerCounters.TotalOutstanding"/>.
    /// For more info refer to https://github.com/StackExchange/StackExchange.Redis/issues/512 .
    /// </summary>
    LeastLoaded
}
