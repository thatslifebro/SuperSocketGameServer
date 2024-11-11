using Microsoft.VisualBasic.FileIO;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameServer;
public class MainServer : AppServer<ClientSession,RequestInfo>
{
    public MainServer()
        : base(new DefaultReceiveFilterFactory<RecvFilter, RequestInfo>())
    {
        NewSessionConnected += new SessionHandler<ClientSession>(OnConnected);
        SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClosed);
        NewRequestReceived += new RequestHandler<ClientSession, RequestInfo>(OnRequestReceived);
    }

    public static ServerOption _serverOption = new();
    SuperSocket.SocketBase.Config.IServerConfig _config;

    PacketProcessor _packetProcessor = new PacketProcessor();
    UserManager _userManager = new UserManager();
    GameRoomManager _gameRoomManager = new GameRoomManager();

    public void InitConfig(ServerOption option)
    {
        _serverOption = option;

        _config = new SuperSocket.SocketBase.Config.ServerConfig
        {
            Name = option.Name,
            Ip = "Any",
            Port = option.Port,
            Mode = SocketMode.Tcp,
            MaxConnectionNumber = option.MaxConnectionNumber,
            MaxRequestLength = option.MaxRequestLength,
            ReceiveBufferSize = option.ReceiveBufferSize,
            SendBufferSize = option.SendBufferSize
        };
    }

    public void CreateStartServer()
    {
        try
        {
            bool bResult = Setup(new SuperSocket.SocketBase.Config.RootConfig(), _config, logFactory: null); // SuperSocket 함수

            if (bResult == false)
            {
                Console.WriteLine("[ERROR] 서버 네트워크 설정 실패");
                return;
            }
            else
            {
                Console.WriteLine("서버 설정 성공");
            }

            GameRoom.SendData = SendData;
            _gameRoomManager.CreateRooms();

            _packetProcessor = new();
            _packetProcessor.Start(_userManager, _gameRoomManager, SendData);

            Start();// SuperSocket 함수
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
        }
    }

    public void ServerStop()
    {
        Stop();

        _packetProcessor.Stop();
    }

    public void SendData(string sessionId, byte[] data)
    {
        var session = GetSessionByID(sessionId); // SuperSocket 함수

        try
        {
            if (session == null)
            {
                return;
            }

            session.Send(data, 0, data.Length); // SuperSocket 함수
        }
        catch (Exception ex)
        {
            session.SendEndWhenSendingTimeOut(); // SuperSocket 함수
            session.Close(); // SuperSocket 함수
        }
    }

    void OnConnected(ClientSession session)
    {
        Console.WriteLine("Session Connected: {0}", session.SessionID);

        InsertPacket(new InternalPacket(session.SessionID, EPacketID.SessionConnect, null));
    }

    void OnClosed(ClientSession session, CloseReason reason)
    {
        Console.WriteLine("Session Closed: {0}, Reason: {1}", session.SessionID, reason);

        InsertPacket(new InternalPacket(session.SessionID, EPacketID.SessionClose, null));
    }

    void OnRequestReceived(ClientSession session, RequestInfo requestInfo)
    {
        //Console.WriteLine("Request Received: {0}", requestInfo.Key);

        InsertPacket(new InternalPacket(session.SessionID, (EPacketID)requestInfo.PacketID, requestInfo.Body));
    }

    void InsertPacket(InternalPacket internalPacket)
    {
        _packetProcessor.InsertPacket(internalPacket);
    }
}

public class ClientSession : AppSession<ClientSession, RequestInfo>
{
}
