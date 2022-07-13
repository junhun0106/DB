namespace RedisLib
{
    public enum LogLevel
    {
        Vervose,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
    }

    interface ILogger
    {
        public static readonly ILogger Default = new NullLogger();

        bool IsEnabled(LogLevel level);
        void LogInformation(string message);
        void LogDebug(string message);
        void LogError(string message);

        class NullLogger : ILogger
        {
            public bool IsEnabled(LogLevel level) => false;
            public void LogDebug(string message) { }
            public void LogError(string message) { }
            public void LogInformation(string message) { }
        }
    }
}
