using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace RedisTestProject
{
    public class RedisProvider
    {
        private static IRedisConnectionPoolManager _redisPoolManager;
        private static RedisConfiguration _redisConfiguration;
        private static ISerializer _serializer;

        public void Initialize(string connectionString)
        {
            _serializer = new NewtonsoftSerializer();
            _redisConfiguration = new RedisConfiguration() {
                ConnectionString = connectionString,
                SyncTimeout = 1000,
                ConnectTimeout = 1000,
                ServerEnumerationStrategy = new ServerEnumerationStrategy {
                    Mode = ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
                },
            };
            _redisPoolManager = new RedisConnectionPoolManager(_redisConfiguration);
        }

        public IRedisDatabase GetClient(int machineId = 0)
        {
            var cacheClient = new RedisClient(_redisPoolManager, _serializer, _redisConfiguration);
            return cacheClient.GetDb(machineId);
        }
    }
}
