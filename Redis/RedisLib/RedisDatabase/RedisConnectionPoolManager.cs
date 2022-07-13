namespace RedisLib
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using StackExchange.Redis;

    sealed partial class RedisConnectionPoolManager
    {
        private readonly StateAwareConnection[] connections;
        private readonly RedisConfiguration redisConfiguration;
        private readonly ILogger logger;
        private readonly Random random = new();
        private bool isDisposed;

        public RedisConnectionPoolManager(RedisConfiguration redisConfiguration, ILogger logger = null)
        {
            this.redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
            this.logger = logger ?? ILogger.Default;

            this.connections = new StateAwareConnection[redisConfiguration.PoolSize];

            for (var i = 0; i < this.redisConfiguration.PoolSize; i++)
            {
                var multiplexer = ConnectionMultiplexer.Connect(this.redisConfiguration.ConfigurationOptions);

                if (this.redisConfiguration.ProfilingSessionProvider != null)
                {
                    multiplexer.RegisterProfiler(this.redisConfiguration.ProfilingSessionProvider);
                }

                this.connections[i] = new StateAwareConnection(multiplexer, this.logger);
            }
        }

        ~RedisConnectionPoolManager()
        {
            this.Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // free managed resources
                foreach (var connection in this.connections)
                {
                    connection.Dispose();
                }
            }

            this.isDisposed = true;
        }

        public IConnectionMultiplexer GetConnection()
        {
            StateAwareConnection connection;

            switch (this.redisConfiguration.ConnectionSelectionStrategy)
            {
                case ConnectionSelectionStrategy.RoundRobin:
                    var nextIdx = this.random.Next(0, this.redisConfiguration.PoolSize);
                    connection = this.connections[nextIdx];
                    break;

                case ConnectionSelectionStrategy.LeastLoaded:
                    connection = this.connections.MinBy(x => x.TotalOutstanding())!;
                    break;

                default:
                    throw new InvalidEnumArgumentException(nameof(this.redisConfiguration.ConnectionSelectionStrategy), (int)this.redisConfiguration.ConnectionSelectionStrategy, typeof(ConnectionSelectionStrategy));
            }

            if (this.logger.IsEnabled(LogLevel.Debug))
            {
                this.logger.LogDebug($"Using connection {connection.Connection.GetHashCode()} with {connection.TotalOutstanding()} outstanding!");
            }

            return connection.Connection;
        }

        /// <inheritdoc/>
        public IEnumerable<IConnectionMultiplexer> GetConnections()
        {
            foreach (var connection in this.connections)
            {
                yield return connection.Connection;
            }
        }
    }
}
