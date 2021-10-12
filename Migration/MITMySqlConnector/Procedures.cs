using MySqlConnector;
using System;
using System.Threading.Tasks;
using ConnectorProxy;
using DbExtensions;
using System.Diagnostics;

namespace MITMySqlConnector
{
    public static class Procedures
    {
        public static async Task<long> mit_sp_test(this IConnectorProxy proxy)
        {
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
            if (sw.ElapsedMilliseconds > 100) {
                Console.WriteLine($"{sp} - {sw.ElapsedMilliseconds}ms");
            }
            return sw.ElapsedMilliseconds;
        }
    }
}
