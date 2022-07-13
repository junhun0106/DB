namespace RedisLib;

static class ServerIteratorFactory
{
    public static IEnumerable<IServer> GetServers(IConnectionMultiplexer multiplexer, ServerEnumerationStrategy serverEnumerationStrategy)
    {
        switch (serverEnumerationStrategy.Mode)
        {
            case ServerEnumerationStrategy.ModeOptions.All:
                return new ServerEnumerable(multiplexer,
                                            serverEnumerationStrategy.TargetRole,
                                            serverEnumerationStrategy.UnreachableServerAction);
            case ServerEnumerationStrategy.ModeOptions.Single:
                var serversSingle = new ServerEnumerable(multiplexer,
                                                         serverEnumerationStrategy.TargetRole,
                                                         serverEnumerationStrategy.UnreachableServerAction);
                return serversSingle.Take(1);
            default:
                throw new NotImplementedException();
        }
    }
}
