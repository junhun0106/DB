using MySqlConnector;
using System;
using System.Threading.Tasks;
using ConnectorProxy;
using DbExtensions;

namespace MITMySqlConnector
{
    public static class Procedures
    {
        public static async Task<int> mit_sp_test(this IConnectorProxy proxy)
        {
            const string sp = "sp_test";

            try {
                using (var conn = new MySqlConnection(proxy.ConnectionString)) {
                    await conn.OpenAsync().ConfigureAwait(false);
                    using (var cmd = conn.CreateCommand()) {
                        cmd.CommandText = sp;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                            while (await reader.ReadAsync().ConfigureAwait(false)) {
                                // select 0 as 'test_result';
                                return reader.GetInt("test_result");
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Console.WriteLine($"{sp} - {e}");
            }

            return -1;
        }
    }
}
