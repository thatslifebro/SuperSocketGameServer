using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class Game
{
    public static Func<string, byte[], bool> NetSendFunc;

    public User? WhitePlayer;
    public User? BlackPlayer;

    GameStatus _gameStatus = GameStatus.None;
    GameResult _gameResult = GameResult.None;
    Turn _turn = Turn.White;

    public static Action<string, byte[]> SendData;

    public Game(Action<string, byte[]> sendData) { SendData = sendData; }

    public void Start(User blackPlayer, User whitePlayer)
    {
        _gameStatus = GameStatus.Playing;
        BlackPlayer = blackPlayer;
        WhitePlayer = whitePlayer;

        // 패킷 전송
        NTFGameStart();
    }

    public void End(GameResult gameResult)
    {
        _gameStatus = GameStatus.End;
        _gameResult = gameResult;
        _turn = Turn.White;
    }

    public void EndExitUser(User exitUser)
    {
        if (exitUser == WhitePlayer)
        {
            End(GameResult.BlackWin);
        }
        else if (exitUser == BlackPlayer)
        {
            End(GameResult.WhiteWin);
        }

        // TODO : 게임 종료 패킷 전송
    }

    public GameResult GetGameResult()
    {
        return _gameResult;
    }

    public void NTFGameStart()
    {
        var packet = new PKTNtfGameStart();
        var UserInfos = new List<(string, string)>();
        UserInfos.Add((WhitePlayer.SessionID, WhitePlayer.UserID));
        UserInfos.Add((BlackPlayer.SessionID, BlackPlayer.UserID));

        packet.UserInfos = UserInfos;

        var bodyData = MessagePackSerializer.Serialize(packet);
        var sendPacket = PacketToBytes.Make(EPacketID.NTFGameStart, bodyData);

        SendData(BlackPlayer.SessionID, sendPacket);
        SendData(WhitePlayer.SessionID, sendPacket);
    }

    public void MovePiece(string sessionID, PKTReqMovePiece request)
    {
        ErrorCode errorCode = ErrorCode.None;
        var opponentID = sessionID == WhitePlayer.SessionID ? BlackPlayer.SessionID : WhitePlayer.SessionID;

        if (_gameStatus != GameStatus.Playing)
        {
            errorCode = ErrorCode.InvalidGameStatus;
        }

        if (_turn == Turn.Black && BlackPlayer.SessionID != sessionID)
        {
            errorCode = ErrorCode.NotYourTurn;
        }

        if (_turn == Turn.White && WhitePlayer.SessionID != sessionID)
        {
            errorCode = ErrorCode.NotYourTurn;
        }

        // TODO : 규칙 체크


        // 패킷 전송
        var response = new PKTResMovePiece() { Result = errorCode };
        var bodyData = MessagePackSerializer.Serialize(response);
        var sendPacket = PacketToBytes.Make(EPacketID.ResMovePiece, bodyData);

        SendData(sessionID, sendPacket);

        // 상대에게 전송
        if( errorCode == ErrorCode.None)
        {
            _turn = _turn == Turn.White ? Turn.Black : Turn.White;

            var bodyData2 = MessagePackSerializer.Serialize(request);
            var sendPacket2 = PacketToBytes.Make(EPacketID.NtfMovePiece, bodyData2);

            SendData(opponentID, sendPacket2);
        }

    }
}
