# C# Super Socket�� �̿��� ü�� ���� ����
�� ������Ʈ�� SuperSocket 1.6������ .NET Core ���� ������Ʈ�� [SuperSocketLite](https://github.com/jacking75/SuperSocketLite)�� �̿��Ͽ� ������ ü�� ���� �����Դϴ�.

## ü�� ���� Ŭ���̾�Ʈ
���� [�����](https://github.com/thatslifebro/WindowsFormChess-LAN-)�� �ֽ��ϴ�. winform�� ���� ���� Chess ���� Ŭ���̾�Ʈ�� ��ũ�Ͽ� �����Ͽ����ϴ�.

## ����
### ��Ʈ��ũ
#### MainServer
- ������ ���� ������ �ϴ� Ŭ������ SuperSocket�� AppServer�� ��ӹ޾� �����Ǿ����ϴ�.
- ������ connect, close, Recv�Ǿ��� �� ó���� �̺�Ʈ�� �����߰�, InternalPacket�� PacketProcessor�� Queue�� �����մϴ�.

#### PacketProcessor
- MainServer���� Queue�� ������ InternalPacket�� ó���ϴ� Ŭ�����Դϴ�.
- �ϳ��� �����带 ����� Queue�� ����� InternalPacket�� ó���մϴ�.
- PacketHandler���� ```Dictionary<EPacketID, Action<InternalPacket>> _handlerMap```�� ��ŶID�� ó���ϴ� �Լ��� ����մϴ�.

### ����
#### UserManager
- ������ ������ �����ϴ� Ŭ�����Դϴ�.
- ```Dictionary<string, User> _userMap```�� ���� ID�� User�� �����մϴ�.

#### GameRoomManager
- ���� ���� ������ �����ϴ� Ŭ�����Դϴ�.
- ```List<GameRoom> _roomList```�� GameRoom�� �������� �����մϴ�.

#### GameRoom
- ������ ������ �����ϴ� Ŭ�����Դϴ�.
- ü�� ��Ģ ���� ������ �ϴ� Chess Ŭ������ ��ӹ޾� ����ϰ� �ֽ��ϴ�.


## ������ ����
- �α��� : ����ID �� �Է¹޾� �α����մϴ�.
- �κ� : �� ����(���, �����ο�), �� ����(���� ID, ������ ��), ��ü ä��
- ���� : �濡 2���� �����ϸ� ������ ����. ������ ���� ����
- ���� �̱��� : ���� ���� ���� �� ó�� �������� �ؾ���. ������ �ϴ� �� ��Ż�� ���� ���� ó�� �ʿ�.

