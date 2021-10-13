using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ConnectorProxy;
using OracleConnectorNet;
using MITMySqlConnector;
using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Validators;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Environments;
using System.Data.Common;

namespace MySqlConnectorMigration
{
    class MyConnectorProxy : IConnectorProxy
    {
        public string ConnectionString { get; set; }
    }

    public class MySqlConnectorBenchmark
    {
        private MyConnectorProxy _proxy;
        private DbConnection _conn;

        [GlobalSetup]
        public void Init()
        {
            _proxy = new MyConnectorProxy();
            _proxy.ConnectionString = "data source=192.168.0.200; database=test; user id=m3web; password=dpaTmflWkd#$; port=3306; sslmode=none; minpoolsize=20; maxpoolsize=100";
            _conn = _proxy.CreateConnection();
        }

        [GlobalCleanup]
        public void Clear()
        {
            _conn.Close();

            MITMySqlConnector.Procedures.ClearPoolAll();
            OracleConnectorNet.Procedures.ClearPoolAll();
        }

        [Benchmark]
        public async Task MIT()
        {
            await _proxy.mit_sp_test(_conn).ConfigureAwait(false);
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
            var customConfig = ManualConfig
                .Create(DefaultConfig.Instance)
                .AddValidator(JitOptimizationsValidator.FailOnError)
                .AddDiagnoser(MemoryDiagnoser.Default)
                .AddColumn(StatisticColumn.AllStatistics)
                .AddJob(Job.Default.WithRuntime(CoreRuntime.Core31))
                .AddJob(Job.Default.WithRuntime(CoreRuntime.Core50))
                .AddExporter(DefaultExporters.Markdown);

            //BenchmarkRunner.Run<MySqlConnectorBenchmark>(customConfig);
            BenchmarkRunner.Run<MySqlClient>(customConfig);
        }
    }
}
