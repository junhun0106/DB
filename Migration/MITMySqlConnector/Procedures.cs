using MySqlConnector;
using System;
using System.Threading.Tasks;
using ConnectorProxy;
using DbExtensions;
using System.Diagnostics;
using MySqlConnector.Logging;
using System.Threading;
using System.Data.Common;

namespace MITMySqlConnector
{
    public static class Procedures
    {
        class CustomLogger : IMySqlConnectorLogger
        {
            public bool IsEnabled(MySqlConnectorLogLevel level)
            {
                return true;
            }

            public void Log(MySqlConnectorLogLevel level, string message, object[] args = null, Exception exception = null)
            {
                if (args?.Length > 0) {
                    message = string.Format(message, args);
                }

                if (exception != null) {
                    message += $"ex:{exception}";
                }

                Console.WriteLine($"[{level}] - {message}");
            }
        }

        class CustomLogProvider : IMySqlConnectorLoggerProvider
        {
            public IMySqlConnectorLogger CreateLogger(string name) => new CustomLogger();
        }

        static Procedures()
        {
            //MySqlConnectorLogManager.Provider = new CustomLogProvider();
        }

        public static void ClearPoolAll()
        {
            MySqlConnection.ClearAllPools();
        }

        public static DbConnection CreateConnection(this IConnectorProxy proxy)
        {
            return new MySqlConnection(proxy.ConnectionString);
        }

        public static async Task<long> mit_sp_test(this IConnectorProxy proxy, int index = 0)
        {
            string logger = $"{nameof(mit_sp_test)}_{index}";
            const string sp = "sp_test";
            var sw = Stopwatch.StartNew();
            try {
                using (var conn = new MySqlConnection(proxy.ConnectionString)) {
                    await conn.OpenAsync().ConfigureAwait(false);
                    using (var cmd = conn.CreateCommand()) {
                        cmd.CommandText = sp;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                            while (await reader.ReadAsync().ConfigureAwait(false)) {
                                // select 0 as 'test_result';
                                var result = reader.GetInt("test_result");
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Console.WriteLine($"{sp} - {e}");
            }
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        public static async Task<long> mit_sp_test(this IConnectorProxy proxy, DbConnection conn, int index = 0)
        {
            string logger = $"{nameof(mit_sp_test)}_{index}";
            const string sp = "sp_test";
            var sw = Stopwatch.StartNew();
            try {
                //await conn.OpenAsync().ConfigureAwait(false);
                using (var cmd = conn.CreateCommand()) {
                    cmd.CommandText = sp;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            // select 0 as 'test_result';
                            var result = reader.GetInt("test_result");
                        }
                    }
                }
                //await conn.CloseAsync().ConfigureAwait(false);
            } catch (Exception e) {
                Console.WriteLine($"{sp} - {e}");
            }
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
    }
}
