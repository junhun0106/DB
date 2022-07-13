namespace RedisLib;

using System.Collections;

/// <summary>
/// The class that allows you to enumerate all the redis servers.
/// </summary>
class ServerEnumerable : IEnumerable<IServer>
{
    private readonly IConnectionMultiplexer multiplexer;
    private readonly ServerEnumerationStrategy.TargetRoleOptions targetRole;
    private readonly ServerEnumerationStrategy.UnreachableServerActionOptions unreachableServerAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEnumerable"/> class.
    /// </summary>
    /// <param name="multiplexer">The redis connection.</param>
    /// <param name="targetRole">The target role.</param>
    /// <param name="unreachableServerAction">The unreachable server strategy.</param>
    public ServerEnumerable(
        IConnectionMultiplexer multiplexer,
        ServerEnumerationStrategy.TargetRoleOptions targetRole,
        ServerEnumerationStrategy.UnreachableServerActionOptions unreachableServerAction)
    {
        this.multiplexer = multiplexer;
        this.targetRole = targetRole;
        this.unreachableServerAction = unreachableServerAction;
    }

    /// <summary>
    /// Return the enumerator of the Redis servers
    /// </summary>
    public IEnumerator<IServer> GetEnumerator()
    {
        foreach (var endPoint in this.multiplexer.GetEndPoints())
        {
            var server = this.multiplexer.GetServer(endPoint);
            if (this.targetRole == ServerEnumerationStrategy.TargetRoleOptions.PreferSlave)
            {
                if (!server.IsReplica)
                {
                    continue;
                }
            }

            if (this.unreachableServerAction == ServerEnumerationStrategy.UnreachableServerActionOptions.IgnoreIfOtherAvailable)
            {
                if (!server.IsConnected || !server.Features.Scan)
                {
                    continue;
                }
            }

            yield return server;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
