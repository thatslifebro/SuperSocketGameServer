# C# Super Socket를 이용한 체스 게임 서버
이 프로젝트는 SuperSocket 1.6버전의 .NET Core 포팅 프로젝트인 [SuperSocketLite](https://github.com/jacking75/SuperSocketLite)를 이용하여 구현된 체스 게임 서버입니다.

## 체스 게임 클라이언트
다음 [저장소](https://github.com/thatslifebro/WindowsFormChess-LAN-)에 있습니다. winform을 통해 만든 Chess 게임 클라이언트를 포크하여 수정하였습니다.

## 구조
### 네트워크
#### MainServer
- 서버의 메인 역할을 하는 클래스로 SuperSocket의 AppServer를 상속받아 구현되었습니다.
- 세션이 connect, close, Recv되었을 때 처리할 이벤트를 구현했고, InternalPacket을 PacketProcessor에 Queue에 저장합니다.

#### PacketProcessor
- MainServer에서 Queue에 저장한 InternalPacket을 처리하는 클래스입니다.
- 하나의 쓰레드를 만들어 Queue에 저장된 InternalPacket을 처리합니다.
- PacketHandler에서 ```Dictionary<EPacketID, Action<InternalPacket>> _handlerMap```에 패킷ID와 처리하는 함수를 등록합니다.

### 게임
#### UserManager
- 유저의 정보를 관리하는 클래스입니다.
- ```Dictionary<string, User> _userMap```에 세션 ID와 User를 저장합니다.

#### GameRoomManager
- 게임 방의 정보를 관리하는 클래스입니다.
- ```List<GameRoom> _roomList```에 GameRoom을 만들어놓고 관리합니다.

#### GameRoom
- 게임의 정보를 저장하는 클래스입니다.
- 체스 규칙 관련 검증을 하는 Chess 클래스를 상속받아 사용하고 있습니다.


## 구현된 내용
- 로그인 : 유저ID 를 입력받아 로그인합니다.
- 로비 : 방 정보(목록, 참가인원), 내 정보(유저 ID, 참가한 방), 전체 채팅
- 게임 : 방에 2명이 참가하면 게임이 시작. 나가면 게임 종료
- 아직 미구현 : 게임 승패 결정 시 처리 서버에서 해야함. 게임을 하다 방 이탈시 게임 승패 처리 필요.

