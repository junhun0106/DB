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


|             Method |        Job |       Runtime |        Library |        Mean |     Error |    StdDev |   StdErr |      Median |         Min |          Q1 |          Q3 |         Max |     Op/s |   Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|------------------- |----------- |-------------- |--------------- |------------:|----------:|----------:|---------:|------------:|------------:|------------:|------------:|------------:|---------:|--------:|--------:|--------:|----------:|
|  OpenFromPoolAsync | Job-WCUHHF |      .NET 5.0 |     MySql.Data |    401.9 us |  15.36 us |  44.56 us |  4.52 us |    396.5 us |    328.0 us |    366.5 us |    431.0 us |    526.0 us | 2,487.95 |       - |       - |       - |     472 B |
|   OpenFromPoolSync | Job-WCUHHF |      .NET 5.0 |     MySql.Data |    433.6 us |  20.90 us |  60.96 us |  6.16 us |    427.0 us |    339.1 us |    378.6 us |    471.3 us |    587.8 us | 2,306.22 |       - |       - |       - |     472 B |
| ExecuteScalarAsync | Job-WCUHHF |      .NET 5.0 |     MySql.Data |    385.2 us |   5.12 us |   4.53 us |  1.21 us |    383.9 us |    379.0 us |    381.8 us |    388.4 us |    393.6 us | 2,595.96 |       - |       - |       - |   3,280 B |
|  ExecuteScalarSync | Job-WCUHHF |      .NET 5.0 |     MySql.Data |    382.1 us |   4.04 us |   3.58 us |  0.96 us |    382.7 us |    376.1 us |    380.4 us |    384.5 us |    388.1 us | 2,616.99 |       - |       - |       - |   3,209 B |
|      ManyRowsAsync | Job-WCUHHF |      .NET 5.0 |     MySql.Data |  2,121.3 us |  42.33 us |  37.52 us | 10.03 us |  2,134.7 us |  2,060.4 us |  2,095.8 us |  2,152.6 us |  2,161.9 us |   471.41 | 15.6250 |       - |       - | 149,265 B |
|       ManyRowsSync | Job-WCUHHF |      .NET 5.0 |     MySql.Data |  2,045.9 us |  16.29 us |  13.61 us |  3.77 us |  2,043.0 us |  2,027.9 us |  2,035.3 us |  2,058.9 us |  2,067.6 us |   488.79 | 15.6250 |       - |       - | 149,121 B |
|  OpenFromPoolAsync | Job-TOYYJH | .NET Core 3.1 |     MySql.Data |    397.6 us |  19.48 us |  56.19 us |  5.74 us |    380.2 us |    327.1 us |    354.1 us |    427.2 us |    528.4 us | 2,515.02 |       - |       - |       - |     472 B |
|   OpenFromPoolSync | Job-TOYYJH | .NET Core 3.1 |     MySql.Data |    354.3 us |   7.89 us |  22.38 us |  2.32 us |    349.4 us |    321.4 us |    336.0 us |    369.2 us |    425.9 us | 2,822.57 |       - |       - |       - |     472 B |
| ExecuteScalarAsync | Job-TOYYJH | .NET Core 3.1 |     MySql.Data |    237.1 us |  14.03 us |  40.91 us |  4.13 us |    225.2 us |    181.4 us |    204.2 us |    268.8 us |    359.5 us | 4,216.74 |       - |       - |       - |   3,360 B |
|  ExecuteScalarSync | Job-TOYYJH | .NET Core 3.1 |     MySql.Data |    376.1 us |   4.99 us |   4.42 us |  1.18 us |    377.2 us |    364.4 us |    374.9 us |    378.8 us |    382.5 us | 2,658.83 |       - |       - |       - |   3,289 B |
|      ManyRowsAsync | Job-TOYYJH | .NET Core 3.1 |     MySql.Data |  2,009.9 us |  16.81 us |  15.72 us |  4.06 us |  2,012.0 us |  1,986.3 us |  1,995.5 us |  2,020.9 us |  2,034.8 us |   497.55 | 15.6250 |       - |       - | 149,277 B |
|       ManyRowsSync | Job-TOYYJH | .NET Core 3.1 |     MySql.Data |  2,021.6 us |  17.28 us |  14.43 us |  4.00 us |  2,019.6 us |  2,000.8 us |  2,010.5 us |  2,030.8 us |  2,047.6 us |   494.65 | 15.6250 |       - |       - | 149,128 B |
|  OpenFromPoolAsync | Job-WCUHHF |      .NET 5.0 | MySqlConnector |    763.3 us |  15.25 us |  44.73 us |  4.50 us |    757.9 us |    697.0 us |    723.5 us |    794.6 us |    889.7 us | 1,310.17 |       - |       - |       - |   3,704 B |
|   OpenFromPoolSync | Job-WCUHHF |      .NET 5.0 | MySqlConnector |    700.1 us |  13.83 us |  36.19 us |  4.05 us |    691.7 us |    655.2 us |    672.4 us |    715.2 us |    793.0 us | 1,428.44 |       - |       - |       - |     792 B |
| ExecuteScalarAsync | Job-WCUHHF |      .NET 5.0 | MySqlConnector |    173.6 us |   3.00 us |   5.49 us |  0.85 us |    172.8 us |    164.4 us |    169.3 us |    176.8 us |    186.4 us | 5,758.89 |  0.2441 |       - |       - |   3,352 B |
|  ExecuteScalarSync | Job-WCUHHF |      .NET 5.0 | MySqlConnector |    160.9 us |   2.76 us |   2.31 us |  0.64 us |    161.3 us |    157.2 us |    159.8 us |    162.8 us |    163.9 us | 6,213.18 |       - |       - |       - |   1,408 B |
|      ManyRowsAsync | Job-WCUHHF |      .NET 5.0 | MySqlConnector |  1,776.6 us |  19.76 us |  18.48 us |  4.77 us |  1,774.6 us |  1,757.1 us |  1,765.7 us |  1,777.0 us |  1,827.7 us |   562.86 |       - |       - |       - |   3,889 B |
|       ManyRowsSync | Job-WCUHHF |      .NET 5.0 | MySqlConnector |  1,387.6 us |   8.01 us |   7.49 us |  1.93 us |  1,387.0 us |  1,374.7 us |  1,383.9 us |  1,393.9 us |  1,398.6 us |   720.64 |       - |       - |       - |   1,809 B |
|  OpenFromPoolAsync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector |    825.7 us |  19.63 us |  57.89 us |  5.79 us |    850.1 us |    728.3 us |    763.5 us |    871.2 us |    946.6 us | 1,211.14 |       - |       - |       - |   3,784 B |
|   OpenFromPoolSync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector |    748.6 us |  14.89 us |  18.29 us |  3.90 us |    751.0 us |    719.3 us |    735.5 us |    761.4 us |    790.7 us | 1,335.88 |       - |       - |       - |     792 B |
| ExecuteScalarAsync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector |    368.2 us |   4.31 us |   4.03 us |  1.04 us |    367.2 us |    360.2 us |    365.8 us |    370.8 us |    375.9 us | 2,715.69 |       - |       - |       - |   3,385 B |
|  ExecuteScalarSync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector |    190.8 us |   4.84 us |  13.42 us |  1.42 us |    186.4 us |    170.9 us |    182.1 us |    197.3 us |    228.2 us | 5,240.08 |       - |       - |       - |   1,408 B |
|     ReadBlobsAsync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector | 19,739.9 us | 174.41 us | 163.14 us | 42.12 us | 19,727.4 us | 19,526.3 us | 19,588.1 us | 19,892.0 us | 19,962.4 us |    50.66 | 31.2500 | 31.2500 | 31.2500 | 232,199 B |
|      ReadBlobsSync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector | 19,698.7 us | 219.12 us | 204.96 us | 52.92 us | 19,592.6 us | 19,510.6 us | 19,558.0 us | 19,847.1 us | 20,166.3 us |    50.76 | 31.2500 | 31.2500 | 31.2500 | 226,449 B |
|      ManyRowsAsync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector |  1,878.9 us |   8.69 us |   8.13 us |  2.10 us |  1,880.3 us |  1,862.2 us |  1,873.5 us |  1,883.4 us |  1,891.3 us |   532.22 |       - |       - |       - |   3,923 B |
|       ManyRowsSync | Job-TOYYJH | .NET Core 3.1 | MySqlConnector |  1,405.4 us |   8.01 us |   7.49 us |  1.93 us |  1,404.9 us |  1,393.8 us |  1,399.2 us |  1,410.0 us |  1,421.2 us |   711.54 |       - |       - |       - |   1,811 B |
