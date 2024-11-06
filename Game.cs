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
    Turn _turn = Turn.Black;

    public void Start(User blackPlayer, User whitePlayer)
    {
        _gameStatus = GameStatus.Playing;
        BlackPlayer = blackPlayer;
        WhitePlayer = whitePlayer;

        // 이런식으로 패킷 보낼 수 있음.

        //var packet = new CSBaseLib.PKTNtfRoomUserList();
        //foreach (var user in _userList)
        //{
        //    packet.UserIDList.Add(user.UserID);
        //}

        //var bodyData = MessagePackSerializer.Serialize(packet);
        //var sendPacket = PacketToBytes.Make(PacketId.NtfRoomUserList, bodyData);

        //NetSendFunc(userNetSessionID, sendPacket);
    }

    public void End(GameResult gameResult)
    {
        _gameStatus = GameStatus.End;
        _gameResult = gameResult;
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
    }

    public GameResult GetGameResult()
    {
        return _gameResult;
    }
    //public ErrorCode MovePiece()
}
