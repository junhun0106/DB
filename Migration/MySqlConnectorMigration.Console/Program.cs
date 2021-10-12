using ConnectorProxy;
using OracleConnectorNet;
using MITMySqlConnector;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Concurrent;
using System;
using System.Linq;
using System.Threading;

namespace MySqlConnectorMigration.Console
{
    internal static class Program
    {
        private class MyConnectorProxy : IConnectorProxy
        {
            public string ConnectionString { get; set; }
        }

        private readonly static IConnectorProxy proxy = new MyConnectorProxy {
            ConnectionString = "data source=192.168.0.200; database=test; user id=m3web; password=dpaTmflWkd#$; port=3306; sslmode=none; minpoolsize=20; maxpoolsize=100",
        };

        private class LatencyData
        {
            public string Key;
            public TimeSpan timeMin;
            public TimeSpan timeMax;
            public TimeSpan timeAvg;

            public int count;
            public int threadMinCount;
            public int threadMaxCount;

            /// <summary>
            /// 절사평균
            /// </summary>
            public TimeSpan TrimmedMean {
                get {
                    if (count < 3) {
                        return timeAvg;
                    }

                    var min = timeMin.Ticks;
                    var max = timeMax.Ticks;
                    var total = timeAvg.Ticks * count;

                    var value = (total - min - max) / (count - 2);
                    return new TimeSpan(value);
                }
            }

            public static TimeSpan GetAvg(TimeSpan prevAvg, TimeSpan x, int n)
            {
                return new TimeSpan(GetAvg(prevAvg.Ticks, x.Ticks, n));
            }

            private static long GetAvg(long prevAvg, long x, int n)
            {
                return ((prevAvg * n) + x) / (n + 1);
            }

            public override string ToString()
            {
                return $"[{Key}] count:{count} min:{timeMin.TotalMilliseconds}ms, max:{timeMax.TotalMilliseconds}ms, avg:{timeAvg.TotalMilliseconds}ms, trimmed_mean:{TrimmedMean.TotalMilliseconds}ms, thread_min:{threadMinCount}, thread_max:{threadMaxCount}";
            }
        }

        private static readonly ConcurrentDictionary<string, LatencyData> _data = new ConcurrentDictionary<string, LatencyData>(System.StringComparer.Ordinal);

        private static async Task Oracle(string key, int index)
        {
            const string prefix = nameof(Oracle);
            var _key = prefix + "_" + key;
            var ems = await proxy.oracle_sp_test().ConfigureAwait(false);
            Latency(_key, ems, index);
        }

        private static async Task MIT(string key, int index)
        {
            const string prefix = nameof(MIT);
            var _key = prefix + "_" + key;
            var ems = await proxy.mit_sp_test().ConfigureAwait(false);
            Latency(_key, ems, index);
        }

        private static void Latency(string key, long ms, int index)
        {
            var ts = TimeSpan.FromMilliseconds(ms);
            _data.AddOrUpdate(key, addFactory, updateFactory);

            LatencyData addFactory(string k)
            {
                return new LatencyData {
                    Key = k,
                    count = 1,
                    timeMin = ts,
                    timeMax = ts,
                    timeAvg = ts,
                    threadMinCount = ThreadPool.ThreadCount,
                    threadMaxCount = ThreadPool.ThreadCount,
                };
            }

            LatencyData updateFactory(string k, LatencyData v)
            {
                if (v.timeMin > ts) {
                    v.timeMin = ts;
                }
                if (v.timeMax < ts) {
                    // System.Console.WriteLine($"[{k},{index}] {v.count}, {v.timeMax.TotalMilliseconds}ms -> {ts.TotalMilliseconds}ms");
                    v.timeMax = ts;
                }

                v.timeAvg = LatencyData.GetAvg(v.timeAvg, ts, v.count);
                v.count++;

                var tc = ThreadPool.ThreadCount;
                if (v.threadMinCount > tc) v.threadMinCount = tc;
                if (v.threadMaxCount < tc) v.threadMaxCount = tc;

                return v;
            }
        }

        private static void Main(string[] _)
        {

            // warm_up
            {
                const int warmUpCount = 10;
                int count = 0;
                const string testName = "warm_up";
                System.Console.WriteLine($"{testName} start... warm_up count : {warmUpCount}");
                var sw = Stopwatch.StartNew();
                Parallel.For(0, warmUpCount, async (index) => {
                    await MIT(testName, index).ConfigureAwait(false);
                    Interlocked.Increment(ref count);
                });
                Parallel.For(0, warmUpCount, async (index) => {
                    await Oracle(testName, index).ConfigureAwait(false);
                    Interlocked.Increment(ref count);
                });

                while (true) {
                    if (count == warmUpCount * 2) {
                        break;
                    }
                }

                sw.Stop();
                System.Console.WriteLine($"{testName} - {sw.ElapsedMilliseconds}ms");
            }
 
            // sync test
            //{
            //    const int testCount = 100;
            //    const string testName = "sync_test";
            //    System.Console.WriteLine($"{testName} start... test count : {testCount}");
            //    var list = new List<Task>(testCount * 2);

            //    var sw = Stopwatch.StartNew();
            //    for (int i = 0; i < testCount; ++i) {
            //        Oracle(testName).Wait();
            //        MIT(testName).Wait();
            //    }

            //    System.Console.WriteLine($"{testName} wait...");
            //    Task.WaitAll(list.ToArray());
            //    sw.Stop();
            //    System.Console.WriteLine($"{testName} - {sw.ElapsedMilliseconds}ms");
            //}

            // async test
            {
                int count = 0;
                const int testCount = 100;
                const string testName = "async_test";
                System.Console.WriteLine($"{testName} start... test count : {testCount}");
                var sw = Stopwatch.StartNew();
                Parallel.For(0, testCount, async (index) => {
                    await Oracle(testName, index).ConfigureAwait(false);
                    Interlocked.Increment(ref count);
                });
                Parallel.For(0, testCount, async (index) => {
                    await MIT(testName, index).ConfigureAwait(false);
                    Interlocked.Increment(ref count);
                });
                sw.Stop();

                while (true) {
                    if (count == testCount * 2) {
                        break;
                    }
                }

                System.Console.WriteLine($"{testName} - {sw.ElapsedMilliseconds}ms");
            }

            foreach (var kv in _data.OrderBy(x => x.Key)) {
                System.Console.WriteLine(kv.Value);
            }
        }
    }
}
