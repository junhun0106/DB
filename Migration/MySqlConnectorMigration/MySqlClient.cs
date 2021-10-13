using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace MySqlConnectorMigration
{
	/// <summary>
	/// https://github.com/mysql-net/MySqlConnector/tree/master/tests/Benchmark
	/// </summary>
	public class MySqlClient
	{
		[Params("MySql.Data", "MySqlConnector")]
		public string Library { get; set; }

		[GlobalSetup]
		public void GlobalSetup()
		{
			using (var connection = new MySqlConnector.MySqlConnection(s_connectionString)) {
				connection.Open();
				using (var cmd = connection.CreateCommand()) {
					cmd.CommandText = @"
create schema if not exists benchmark;

drop table if exists benchmark.integers;
create table benchmark.integers (value int not null primary key);
insert into benchmark.integers(value) values (0),(1),(2),(3),(4),(5),(6),(7),(8),(9);";

					cmd.ExecuteNonQuery();
				}
			}

			s_connectionString += ";database=benchmark";

			var mySqlData = new MySql.Data.MySqlClient.MySqlConnection(s_connectionString);
			mySqlData.Open();
			m_connections.Add("MySql.Data", mySqlData);

			var mySqlConnector = new MySqlConnector.MySqlConnection(s_connectionString);
			mySqlConnector.Open();
			m_connections.Add("MySqlConnector", mySqlConnector);

			Connection = m_connections[Library];
		}

		[GlobalCleanup]
		public void GlobalCleanup()
		{
			foreach (var connection in m_connections.Values)
				connection.Dispose();
			m_connections.Clear();
			MySqlConnector.MySqlConnection.ClearAllPools();
			MySql.Data.MySqlClient.MySqlConnection.ClearAllPools();
		}

		[Benchmark]
		public async Task OpenFromPoolAsync()
		{
			Connection.Close();
			await Connection.OpenAsync().ConfigureAwait(false);
		}

		[Benchmark]
		public async Task ExecuteScalarAsync()
		{
			using var cmd = Connection.CreateCommand();
			cmd.CommandText = c_executeScalarSql;
			await cmd.ExecuteScalarAsync().ConfigureAwait(false);
		}

		private const string c_executeScalarSql = "select max(value) from integers;";

		[Benchmark] public Task ManyRowsAsync() => ReadAllRowsAsync(c_manyRowsSql);

		private const string c_manyRowsSql = "select * from integers a join integers b join integers c;";

		private async Task<int> ReadAllRowsAsync(string sql)
		{
			int total = 0;
			using (var cmd = Connection.CreateCommand()) {
				cmd.CommandText = sql;
				using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
				do {
					while (await reader.ReadAsync().ConfigureAwait(false)) {
						if (reader.FieldCount > 1)
							total += reader.GetInt32(1);
					}
				} while (await reader.NextResultAsync().ConfigureAwait(false));
			}
			return total;
		}

		private DbConnection Connection { get; set; }

		// TODO: move to config file
		//static string s_connectionString = "data source=192.168.0.200; database=test; user id=m3web; password=dpaTmflWkd#$; port=3306; ssl mode=none;Use Affected Rows=true;Connection Reset=false;Default Command Timeout=0;AutoEnlist=false;";
		static string s_connectionString = "data source=192.168.0.200; database=test; user id=m3web; password=dpaTmflWkd#$; port=3306; sslmode=none; minpoolsize=20; maxpoolsize=100";

		Dictionary<string, DbConnection> m_connections = new Dictionary<string, DbConnection>(StringComparer.Ordinal);
	}
}
