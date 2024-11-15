using Microsoft.Extensions.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;


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

    #region OpenTelemetry

    public OpenTelemetryHelper _openTelemetryHelper = new();

    #endregion

    public static ServerOption _serverOption = new();
    SuperSocket.SocketBase.Config.IServerConfig? _config;

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
            bool bResult = Setup(new SuperSocket.SocketBase.Config.RootConfig(), _config, logFactory: null);

            if (bResult == false)
            {
                _openTelemetryHelper.logger.LogInformation("서버 설정 실패");
                return;
            }
            else
            {
                _openTelemetryHelper.logger.LogInformation("서버 설정 성공");
            }

            GameRoom.SendData = SendData;
            _gameRoomManager.CreateRooms();

            _packetProcessor = new();
            _packetProcessor.Start(_userManager, _gameRoomManager, SendData, _openTelemetryHelper);

            Start();
        }
        catch (Exception ex)
        {
            _openTelemetryHelper.logger.LogError($"서버 생성 실패: {ex}");
        }
    }

    public void ServerStop()
    {
        Stop();

        _packetProcessor.Stop();
    }

    public void SendData(string sessionId, byte[] data)
    {
        var session = GetSessionByID(sessionId);

        try
        {
            if (session == null)
            {
                return;
            }

            session.Send(data, 0, data.Length);

            _openTelemetryHelper.instrumentation.OnDataSent(data.Length);
        }
        catch (Exception)
        {
            session.SendEndWhenSendingTimeOut();
            session.Close();

            _openTelemetryHelper.instrumentation.OnError();
        }
    }

    void OnConnected(ClientSession session)
    {
        _openTelemetryHelper.logger.LogInformation($"Session Connected: {session.SessionID}");

        _openTelemetryHelper.instrumentation.OnClientConnected();

        InsertPacket(new InternalPacket(session.SessionID, EPacketID.SessionConnect, null));
    }

    void OnClosed(ClientSession session, CloseReason reason)
    {
        _openTelemetryHelper.logger.LogInformation($"Session Closed: {session.SessionID}, Reason: {reason}");

        _openTelemetryHelper.instrumentation.OnClientDisconnected();

        InsertPacket(new InternalPacket(session.SessionID, EPacketID.SessionClose, null));
    }

    void OnRequestReceived(ClientSession session, RequestInfo requestInfo)
    {
        InsertPacket(new InternalPacket(session.SessionID, (EPacketID)requestInfo.PacketID, requestInfo.Body));

        _openTelemetryHelper.logger.LogInformation($"OnRequestReceived: {requestInfo.PacketID}, SessionID : {session.SessionID}");
    }

    void InsertPacket(InternalPacket internalPacket)
    {
        _packetProcessor.InsertPacket(internalPacket);
    }
}

public class ClientSession : AppSession<ClientSession, RequestInfo>
{
}
