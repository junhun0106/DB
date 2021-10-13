* https://github.com/mysql-net/MySqlConnector/tree/master/tests/Benchmark
* 위 링크를 정상화시켜서 한 테스트
* LongBlob에 경우는 일반적으로 사용하지 않으므로 테스트에서 제외
* sync는 우리 프로젝트 환경에서는 사용하지 않으므로 테스트에서 제외

---

* 결과만 보면 내가 했던 테스트들에서 MIT가 더 느린 결과가 나오는게 맞다.
* https://mysqlconnector.net/
  * 이곳에서 보여주는 벤치마크 스크린샷은 .net core 2.1, .net framework 4.7.2를 대상으로 함
  * MySql.Data도 현재 사용하고 있는 버전보다 낮음
  * MySql.Data도 .net 버전이 오르면서 업그레이드를 시도했을 가능성이 높음

---

|             Method |        Job |       Runtime |        Library |        Mean |     Error |    StdDev |   StdErr |      Median |         Min |          Q1 |          Q3 |         Max |     Op/s |   Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|------------------- |----------- |-------------- |--------------- |------------:|----------:|----------:|---------:|------------:|------------:|------------:|------------:|------------:|---------:|--------:|--------:|--------:|----------:|
|  OpenFromPoolAsync | Job-WCUHHF |      .NET 5.0 |     MySql.Data |    401.9 us |  15.36 us |  44.56 us |  4.52 us |    396.5 us |    328.0 us |    366.5 us |    431.0 us |    526.0 us | 2,487.95 |       - |       - |       - |     472 B |
| ExecuteScalarAsync | Job-WCUHHF |      .NET 5.0 |     MySql.Data |    385.2 us |   5.12 us |   4.53 us |  1.21 us |    383.9 us |    379.0 us |    381.8 us |    388.4 us |    393.6 us | 2,595.96 |       - |       - |       - |   3,280 B |
|      ManyRowsAsync | Job-WCUHHF |      .NET 5.0 |     MySql.Data |  2,121.3 us |  42.33 us |  37.52 us | 10.03 us |  2,134.7 us |  2,060.4 us |  2,095.8 us |  2,152.6 us |  2,161.9 us |   471.41 | 15.6250 |       - |       - | 149,265 B |
|  OpenFromPoolAsync | Job-TOYYJH | .NET Core 3.1 |     MySql.Data |    397.6 us |  19.48 us |  56.19 us |  5.74 us |    380.2 us |    327.1 us |    354.1 us |    427.2 us |    528.4 us | 2,515.02 |       - |       - |       - |     472 B |
| ExecuteScalarAsync | Job-TOYYJH | .NET Core 3.1 |     MySql.Data |    237.1 us |  14.03 us |  40.91 us |  4.13 us |    225.2 us |    181.4 us |    204.2 us |    268.8 us |    359.5 us | 4,216.74 |       - |       - |       - |   3,360 B |
|      ManyRowsAsync | Job-TOYYJH | .NET Core 3.1 |     MySql.Data |  2,009.9 us |  16.81 us |  15.72 us |  4.06 us |  2,012.0 us |  1,986.3 us |  1,995.5 us |  2,020.9 us |  2,034.8 us |   497.55 | 15.6250 |       - |       - | 149,277 B |
|  OpenFromPoolAsync | Job-WCUHHF |      .NET 5.0 | MySqlConnector |    763.3 us |  15.25 us |  44.73 us |  4.50 us |    757.9 us |    697.0 us |    723.5 us |    794.6 us |    889.7 us | 1,310.17 |       - |       - |       - |   3,704 B |
| ExecuteScalarAsync | Job-WCUHHF |      .NET 5.0 | MySqlConnector |    173.6 us |   3.00 us |   5.49 us |  0.85 us |    172.8 us |    164.4 us |    169.3 us |    176.8 us |    186.4 us | 5,758.89 |  0.2441 |       - |       - |   3,352 B |
|      ManyRowsAsync | Job-WCUHHF |      .NET 5.0 | MySqlConnector |  1,776.6 us |  19.76 us |  18.48 us |  4.77 us |  1,774.6 us |  1,757.1 us |  1,765.7 us |  1,777.0 us |  1,827.7 us |   562.86 |       - |       - |       - |   3,889 B |
|  OpenFromPoolAsync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector |    825.7 us |  19.63 us |  57.89 us |  5.79 us |    850.1 us |    728.3 us |    763.5 us |    871.2 us |    946.6 us | 1,211.14 |       - |       - |       - |   3,784 B |
| ExecuteScalarAsync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector |    368.2 us |   4.31 us |   4.03 us |  1.04 us |    367.2 us |    360.2 us |    365.8 us |    370.8 us |    375.9 us | 2,715.69 |       - |       - |       - |   3,385 B |
|      ManyRowsAsync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector |  1,878.9 us |   8.69 us |   8.13 us |  2.10 us |  1,880.3 us |  1,862.2 us |  1,873.5 us |  1,883.4 us |  1,891.3 us |   532.22 |       - |       - |       - |   3,923 B |
