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
			