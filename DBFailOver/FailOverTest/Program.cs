using MySql.Data.MySqlClient;
using Polly;
using System;
using System.Threading;

namespace FailOverTest
{
    public class MySqlForcedException : Exception
    {
    }

    public static class MySqlExceptionHandler
    {
        public static bool IsFailOverException(Exception e)
        {
            if (e is MySqlException msex) {
                var errorCode = (MySqlErrorCode)msex.Number;
                switch (errorCode) {
                    case MySqlErrorCode.None:
                    case MySqlErrorCode.ConnectionCountError:
                    case MySqlErrorCode.UnableToConnectToHost:
                    case MySqlErrorCode.ServerShutdown:
                    case MySqlErrorCode.NormalShutdown:
                    case MySqlErrorCode.ShutdownComplete:
                    case MySqlErrorCode.AbortingConnection:
                    case MySqlErrorCode.NewAbortingConnection:
                        return true;
                }
            } else if (e is MySqlForcedException) {
                return true;
            }

            return false;
        }
    }

    class Program
    { 
        static void Main(string[] args)
        {
            var now = DateTime.Now;

            // DB FailOver 시 어느 정도의 시간이 필요하다 (최대 1분까지도 걸릴 수 있다)
            // 이미 FailOver가 감지 된 DB에 요청을 하는 건 비용 낭비
            // DB에 요청하기 전에 지정 된 시간 만큼 대기 할 필요가 있다
            // 만약에 웹 요청을 통한다면, Middleware를 이용하여 앞에서 걸러 낼 수 있다
            var policy = Policy.Handle<Exception>(MySqlExceptionHandler.IsFailOverException)
                .CircuitBreaker(1, TimeSpan.FromSeconds(2));

            while (true) {
                Thread.Sleep(500);

                try {
                    policy.Execute(Call);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
                var dt = (DateTime.Now - now).TotalSeconds;
                if (dt > 10) {
                    break;
                }
            }

            /*
             test_result - call
             Unable to connect to any of the specified MySQL hosts.
             The circuit is now open and is not allowing calls.
             The circuit is now open and is not allowing calls.
             The circuit is now open and is not allowing calls.
             test_result - call
             Unable to connect to any of the specified MySQL hosts.
             The circuit is now open and is not allowing calls.
             The circuit is now open and is not allowing calls.
             The circuit is now open and is not allowing calls.
             test_result - call
             Unable to connect to any of the specified MySQL hosts.
             */

            Console.Write("test complete...");
            Console.Read();
        }

        private static void Call()
        {
            const string connectString = "data source=localhost; database=test; user id=test; password=test; port=3306; sslmode=none";
            const string sp = "test_result";
            MySqlConnection conn = null;
            try {
                // CircuitBreaker로 인해 특정 시간 이후에만 호출
                Console.WriteLine($"{sp} - call");
                conn = new MySqlConnection(connectString);
                conn.Open(); // await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sp;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    var _ = reader.GetInt32(reader.GetOrdinal("test_result"));
                }
            }
            // catch (MySqlForcedException e) {
            //    if (conn != null) {
            //        MySqlConnection.ClearPool(conn);
            //    }
            //    throw;
            //}
            catch (MySqlException e) when (MySqlExceptionHandler.IsFailOverException(e)) {
                if (conn != null) {
                    // DNS 캐시 등 초기화 FailOver를 감지한 경우 초기화 처리가 진행 되어야 한다
                    MySqlConnection.ClearPool(conn);
                }
                // NOTE : 최상위까지 전파하여, 후처리를 진행 해야 한다
                throw;
            } catch (Exception e) {
                // error - unknown error
                Console.WriteLine(e);
            } finally {
                conn?.Dispose();
            }
        }
    }
}
