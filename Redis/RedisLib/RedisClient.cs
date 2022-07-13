global using StackExchange.Redis;

namespace RedisLib
{
    using System.Text;
    using System.Text.Json;

    // https://github.com/imperugo/StackExchange.Redis.Extensions
    public class RedisProvider
    {
        class NewtonsoftSerializer : ISerializer
        {
            static readonly JsonSerializerOptions @default = new JsonSerializerOptions();

            private readonly JsonSerializerOptions options;

            public NewtonsoftSerializer(JsonSerializerOptions settings = null)
            {
                this.options = settings ?? @default;
            }

            public string Serialize<T>(T item) => JsonSerializer.Serialize(item, this.options);

            public T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, this.options);
        }

        private readonly RedisConnectionPoolManager pool;
        private readonly RedisConfiguration config;
        private readonly ISerializer serializer;

        public RedisProvider(string connectionString)
        {
            this.serializer = new NewtonsoftSerializer();
            this.config = new()
            {
                ConnectionString = connectionString,
                SyncTimeout = 1000,
                ConnectTimeout = 1000,
                ServerEnumerationStrategy = new ServerEnumerationStrategy
                {
                    Mode = ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
                },
            };
            this.pool = new RedisConnectionPoolManager(this.config);
        }

        public IRedisDatabase Get(int dbNumber = 0)
        {
            return new RedisDatabase(this.pool,
                                     this.serializer,
                                     this.config.ServerEnumerationStrategy,
                                     dbNumber,
                                     this.config.MaxValueLength,
                                     this.config.KeyPrefix);
        }
    }
}
