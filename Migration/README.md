#### C# MySqlConnector Migration

* 변경 이유
	* 웹 사이트를 돌아다니다보니, 아래 링크에 있는 글을 읽게 됨
		* https://mysqlconnector.net/
	* 확인해보니 MIT에서 만든 MySqlConnector에 대한 벤치마크 결과.
		* 내가 주로 사용하던 Oracle의 Connector/NET에 대한 평가도 함께 실려 있음.
		* 글에서 자신들은 Oracle의 Connector/NET을 기반으로 하지 않았다고 되어 있으나
			* 대부분의 클래스 네이밍들이 모두 똑같음.
	* 만약 벤치마크 결과가 사실이고 실제 게임에서도 Latency를 줄일 수 있다면
		* 네이밍이 같으므로 변경에 소모되는 시간을 아낄 수 있다고 판단함.
	* 간단하게 사용 예제에 대한 벤치마크를 돌려보고 확인해보자.
	
* 참고
	* 내가 주로 다뤘던 프로젝트는 .net standard 2.0과 .net core 3.1을 주로 사용 하였다.
	* 시간이 남는다면 .net 5.0에서는 어떤 결과가 있는지도 확인해보자.
			
			
---

* Mean                 : Arithmetic mean of all measurements
* Error                : Half of 99.9% confidence interval
* StdDev               : Standard deviation of all measurements
* Completed Work Items : The number of work items that have been processed in ThreadPool (per single operation)
* Lock Contentions     : The number of times there was contention upon trying to take a Monitor's lock (per single operation)
* Gen 0                : GC Generation 0 collects per 1000 operations
* Allocated            : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
* 1 us                 : 1 Microsecond (0.000001 sec)


---

#### 1차 시도


* test sp

```
 select 0 as 'test_result';
```

| Method |       Mean |    Error |   StdDev | Completed Work Items | Lock Contentions |  Gen 0 | Allocated |
|------- |-----------:|---------:|---------:|---------------------:|-----------------:|-------:|----------:|
|    MIT | 1,146.9 us | 17.36 us | 16.24 us |               3.0039 |                - |      - |      9 KB |
| Oracle |   954.3 us | 13.91 us | 12.33 us |               0.0039 |                - | 1.9531 |     25 KB |

* 아무것도 변경하지 않은 상태에서는 MIT가 더 느리게 나왔다.
	* 더 빠르게하기 위해서는 어떤 방법들을 제공하는지 찾아보고 변경하는 것이 너무 많다면 그대로 포기. 


---

#### 2차 시도
* MIT MySqlConnector에 설명을 보면 비동기 강화, Memory Allocated 강화를 볼 수 있다
	*  Memory Allocated 경우 1차 시도에서 줄어든 것 확인
* Multi Thread 환경에서 테스트 해보자

```
1차 테스트
[MIT_async_test] 	count:10000 min:1ms, 	max:978ms, 	avg:691.1322ms, trimmed_mean:691.1725ms, thread_min:24, thread_max:44
[MIT_warm_up] 		count:10 min:72ms, 	max:116ms, 	avg:100.2997ms, trimmed_mean:101.8746ms, thread_min:24, thread_max:24
[Oracle_async_test] 	count:9999 min:0ms, 	max:39ms, 	avg:1.6713ms, trimmed_mean:1.6677ms, thread_min:24, thread_max:42
[Oracle_warm_up] 	count:10 min:4ms, 	max:355ms, 	avg:39.7998ms, trimmed_mean:4.8747ms, thread_min:3, thread_max:22
```

```
2차 테스트
[MIT_async_test] count:9993 min:1ms, max:932ms, avg:3.0005ms, trimmed_mean:2.9077ms, thread_min:24, thread_max:38
[MIT_warm_up] count:10 min:61ms, max:88ms, avg:93.2999ms, trimmed_mean:97.9998ms, thread_min:24, thread_max:24
[Oracle_async_test] count:9996 min:1ms, max:29ms, avg:2.5049ms, trimmed_mean:2.5023ms, thread_min:24, thread_max:38
[Oracle_warm_up] count:10 min:4ms, max:310ms, avg:35.6998ms, trimmed_mean:5.3747ms, thread_min:3, thread_max:18
```

* MIT가 느리다.
* max가 튀는 부분이 존재 한다

```
[MIT_async_test,2590] 9978, 31ms -> 1550ms
[MIT_async_test,5343] 9978, 31ms -> 932ms
```

* 세밀하게 보면 튀는 부분을 제외하면 평균적으로 MIT가 훨씬 빠르다. 왜 튀는지 좀 더 살펴 볼 필요가 있겠다.
* sp 호출 자체를 확인 했을 때는 오래 걸리지 않는다.
	* Task가 await 될 때 문제가 되거나(ThreadPool 반환), GC.WaitTime에 걸렸거나 ...

#### 3차 시도

* https://github.com/mysql-net/MySqlConnector/tree/master/tests/Benchmark
* https://github.com/mysql-net/MySqlConnector/tree/master/tests/MySqlConnector.Performance
* MySqlConnector 팀에서 만든 벤치마크와 Pref 프로젝트 참고

* .NET 버전 차이가 아닌가 의심. BenchmarkDotNet에 여러 런타임에서 돌릴 수 있도록 구성
* 성능이 나아지지 않음.

| Method |        Job |       Runtime |     Mean |     Error |    StdDev |    StdErr |   Median |       Min |        Q1 |       Q3 |      Max |  Op/s |  Gen 0 | Allocated |
|------- |----------- |-------------- |---------:|----------:|----------:|----------:|---------:|----------:|----------:|---------:|---------:|------:|-------:|----------:|
|    MIT | Job-IBXAQC |      .NET 5.0 | 1.256 ms | 0.0249 ms | 0.0610 ms | 0.0072 ms | 1.278 ms | 1.1484 ms | 1.2008 ms | 1.296 ms | 1.412 ms | 796.5 |      - |      9 KB |
| Oracle | Job-IBXAQC |      .NET 5.0 | 1.019 ms | 0.0186 ms | 0.0261 ms | 0.0050 ms | 1.018 ms | 0.9730 ms | 0.9996 ms | 1.035 ms | 1.085 ms | 981.1 | 1.9531 |     25 KB |
|    MIT | Job-YVPNEH | .NET Core 3.1 | 1.162 ms | 0.0194 ms | 0.0182 ms | 0.0047 ms | 1.161 ms | 1.1268 ms | 1.1537 ms | 1.174 ms | 1.195 ms | 860.3 |      - |      9 KB |
| Oracle | Job-YVPNEH | .NET Core 3.1 | 1.027 ms | 0.0203 ms | 0.0344 ms | 0.0057 ms | 1.027 ms | 0.9589 ms | 1.0050 ms | 1.054 ms | 1.084 ms | 973.4 | 1.9531 |     25 KB |

* 다음으로 의심가는 건 MIT에 Pref Test들을 보면 Connection을 1개만 생성하고 사용하는 걸 볼 수 있음. Oracle이 권장하는 using () { }으로는 성능이 떨어지는 것이 아닐까 ?
	* https://mysqlconnector.net/tutorials/migrating-from-connector-net/
	* 해당 링크를 꼼꼼히 읽어봐야  

---

* https://github.com/mysql-net/MySqlConnector/tree/master/tests/Benchmark
* 위 링크를 정상화시켜서 한 테스트
* LongBlob에 경우는 일반적으로 사용하지 않으므로 테스트에서 제외
* sync는 우리 프로젝트 환경에서는 사용하지 않으므로 테스트에서 제외

---

* https://mysqlconnector.net/
  * 이곳에서 보여주는 벤치마크 스크린샷은 .net core 2.1, .net framework 4.7.2를 대상으로 함
  * MySql.Data도 현재 사용하고 있는 버전보다 낮음
  * MySql.Data도 .net 버전이 오르면서 업그레이드를 시도했을 가능성이 높음

---

* OpenFromPool MySql.Data > MySqlConnector
* ManyRowsAsync MySqlConnector > MySql.Data
* 두 개의 차이가 Row를 많이 읽는 경우라 할지라도, Open -> Many Row Read를 하는 경우이므로 간단한 변경은 불가능함
  * Connection을 미리 Open하고 계속 물고 있는 형태로 변경이 필요
  * 압도적인 성능 차이가 아니라면 굳이 변경에 시간을 소모 할 필요 없음
  * Many Row Read에 경우 Row를 많이 읽지 않도록 DB 스키마나 sp를 변경하는게 훨씬 비용 소모 적음(빨리 수정)
    * 애초에 그렇게 많이 읽는 경우도 없음  

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


* **결론 : 굳이 변경 할 필요 없으며, MySql.Data(Oracle Connector/NET)도 .net 스펙을 충분히 잘 따라오고 있는 것으로 보임.**

