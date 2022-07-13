namespace RedisLib
{
    using System;
    using StackExchange.Redis;

    sealed partial class RedisConnectionPoolManager
    {
        sealed class StateAwareConnection
        {
            private readonly ILogger logger;

            public readonly IConnectionMultiplexer Connection;

            public StateAwareConnection(IConnectionMultiplexer multiplexer, ILogger logger)
            {
                this.Connection = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
                this.Connection.ConnectionFailed += this.ConnectionFailed;
                this.Connection.ConnectionRestored += this.ConnectionRestored;
                this.Connection.InternalError += this.InternalError;
                this.Connection.ErrorMessage += this.ErrorMessage;

                this.logger = logger;
            }

            public long TotalOutstanding() => this.Connection.GetCounters().TotalOutstanding;

            public bool IsConnected() => !this.Connection.IsConnecting;

            public void Dispose()
            {
                this.Connection.ConnectionFailed -= this.ConnectionFailed;
                this.Connection.ConnectionRestored -= this.ConnectionRestored;
                this.Connection.InternalError -= this.InternalError;
                this.Connection.ErrorMessage -= this.ErrorMessage;

                this.Connection.Dispose();
            }

            private void ConnectionFailed(object sender, ConnectionFailedEventArgs e)
            {
                this.logger.LogError($"Redis connection error {e.FailureType}, {e.Exception}");
            }

            private void ConnectionRestored(object sender, ConnectionFailedEventArgs e)
            {
                this.logger.LogInformation("Redis connection error restored");
            }

            private void InternalError(object sender, InternalErrorEventArgs e)
            {
                this.logger.LogError($"Redis internal error {e.Origin}, {e.Exception}");
            }

            private void ErrorMessage(object sender, RedisErrorEventArgs e)
            {
                this.logger.LogError($"Redis error: {e.Message}");
            }
        }
    }
}
