using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ConnectorProxy;
using DbExtensions;
using System.Diagnostics;

#pragma warning disable IDE1006 // 명명 스타일
namespace OracleConnectorNet
{
    public static class Procedures
    {
        public static async Task<long> oracle_sp_test(this IConnectorProxy proxy)
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
            return sw.ElapsedMilliseconds;
        }
    }
}
#pragma warning restore IDE1006 // 명명 스타일
