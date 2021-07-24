using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        // 파티션 성능 테스트
        private static void TestPartition()
        {
            // 파티션을 한 테이블과 그렇지 않은 테이블에 대해 select를 해보았을 때의 성능 차이

            const string connectString = "data source=localhost; database=kjh; user id=root; password=1234567890; port=3306; sslmode=none";

            List<double> quest_classId_elapsed = new List<double>();
            List<double> quest_partition_elapsed = new List<double>();
            const int testCount = 1000;
            var taskList = new List<Task> {
                Task.Factory.StartNew(() => {
                    Parallel.For(0, testCount, (index) => {
                        Console.WriteLine($"{index} parallel start");
                        using (var conn = new MySqlConnection(connectString)) {
                            conn.Open();
                            using (var cmd = conn.CreateCommand()) {
                                cmd.CommandText = "sp_call_update_no_pt";
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                var sw = Stopwatch.StartNew();
                                cmd.ExecuteNonQuery();
                                sw.Stop();
                                lock (quest_classId_elapsed) {
                                    quest_classId_elapsed.Add(sw.ElapsedMilliseconds);
                                }
                            }
                        }
                    });
                }),
                Task.Factory.StartNew(() => {
                    Parallel.For(0, testCount, (index) => {
                        Console.WriteLine($"{index} parallel start");
                        using (var conn = new MySqlConnection(connectString)) {
                            conn.Open();
                            using (var cmd = conn.CreateCommand()) {
                                cmd.CommandText = "sp_call_update";
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                var sw = Stopwatch.StartNew();
                                cmd.ExecuteNonQuery();
                                sw.Stop();
                                lock (quest_partition_elapsed) {
                                    quest_partition_elapsed.Add(sw.ElapsedMilliseconds);
                                }
                            }
                        }
                    });
                })
            };

            Task.WaitAll(taskList.ToArray());

            var sumClassIdElapsed = quest_classId_elapsed.Sum(x => x);
            var sumPartitionElapsed = quest_partition_elapsed.Sum(x => x);

#if DEBUG
            Console.WriteLine("Current Test Mode : DEBUG");
#else
            Console.WriteLine("Current Test Mode : RELEASE");
#endif
            Console.WriteLine($"Test Count : {testCount}");
            Console.WriteLine("\t testCount 1 당 - 파티션이 where절에 포함 되지 않은 경우와 포함 된 경우 각각 한 번 씩");
            Console.WriteLine($"파티션이 없는 테이블 : Avg - {sumClassIdElapsed / quest_classId_elapsed.Count}ms");
            Console.WriteLine($"파티션이 있는 테이블 : Avg - {sumPartitionElapsed / quest_partition_elapsed.Count}ms");

            Console.ReadLine();

            // 파티션을 나누지 않더라도 인덱스 key만 잘 지정되어 있으면 성능 차이는 크게 없다
            // 유지/관리면에서는 파티션을 나누는 것이 훨씬 유리하다
        }

        private static void FindNeverUsedTable()
        {
            const string connectString = "data source=localhost; database=kjh; user id=root; password=1234567890; port=3306; sslmode=none";

            var conn = new MySqlConnection(connectString);

            conn.Open();

            var list = new List<string>();

            using (var cmd = conn.CreateCommand()) {
                cmd.CommandText = "select TABLE_NAME " +
                                  "from information_schema.`TABLES` " +
                                  "where TABLE_SCHEMA = 'kjh'";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        var tableName = reader.GetString(reader.GetOrdinal("TABLE_NAME"));
                        list.Add(tableName);
                    }
                }
            }

            Console.WriteLine("add all table complete");


            var spList = new HashSet<string>();

            foreach (var tableName in list) {
                using (var cmd = conn.CreateCommand()) {
                    cmd.CommandText = "select ROUTINE_NAME " +
                                      "from information_schema.routines " +
                                     $"where LOWER(ROUTINE_DEFINITION) LIKE '%{tableName}%'";


                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            spList.Add(reader.GetString(reader.GetOrdinal("ROUTINE_NAME")));
                        }
                    }
                }
            }

            Console.WriteLine("add use sp complete");


            var allSp = new HashSet<string>();
            using (var cmd = conn.CreateCommand()) {
                cmd.CommandText = "select ROUTINE_NAME " +
                                  "from information_schema.ROUTINES " +
                                 $"where ROUTINE_SCHEMA = 'kjh'";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        var name = reader.GetString(reader.GetOrdinal("ROUTINE_NAME"));
                        allSp.Add(name);
                    }
                }
            }

            Console.WriteLine("add all sp complete");

            foreach (var sp in allSp) {
                if (spList.Contains(sp)) continue;
                Console.WriteLine($"unused : {sp}");
            }

            foreach (var sp in spList) {
                if (allSp.Contains(sp)) continue;
                Console.WriteLine($"? : {sp}");
            }

            conn.Close();
        }

        static void Main(string[] args)
        {
        }
    }
}
