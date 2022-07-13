# RedisLib

#### Link

* [StackExchange.Redis.Extensions](https://github.com/imperugo/StackExchange.Redis.Extensions)

#### Explain

* 파라미터 네이밍 수정
	* API마다 같은 파라미터지만 다른 네이밍 사용하던 것 수정
	* item, value 등 헷갈리는 네이밍들 수정

* json string -> byte[] serialize에서 string serialize로 변경.
	* stackExchange.Redis.RedisValue는 string이 main.
	* byte[] 변환 시에 CG 해결
	
* HashScanAsync 버전 추가
	* HashScan 블로킹 API -> 비동기 API 사용 할 수 있도록 함
	
* 지속적으로 업데이트 하자!
	
	