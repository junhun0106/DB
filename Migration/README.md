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
