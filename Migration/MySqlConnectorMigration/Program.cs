using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ConnectorProxy;
using OracleConnectorNet;
using MITMySqlConnector;
using System;
using System.Threading.Tasks;

namespace MySqlConnectorMigration
{
    class MyConnectorProxy : IConnectorProxy
    {
        public string ConnectionString { get; set; }
    }

    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class MySqlConnectorBenchmark
    {
        private MyConnectorProxy _proxy;

        [GlobalSetup]
        public void Init()
        {
            _proxy = new MyConnectorProxy();
            _proxy.ConnectionString = "data source=192.168.0.200; database=test; user id=m3web; password=dpaTmflWkd#$; port=3306; sslmode=none; minpoolsize=20; maxpoolsize=100";
        }

        [Benchmark]
        public async Task MIT()
        {
            await _proxy.mit_sp_test().ConfigureAwait(false);
        }

        [Benchmark]
        public async Task Oracle()
        {
            await _proxy.oracle_sp_test().ConfigureAwait(false);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(MySqlConnectorBenchmark).Assembly).Run(args);
            Console.ReadLine();
        }
    }
}
