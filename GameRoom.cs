using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class GameRoom
{
    public UInt16 RoomID { get; private set; }
    public Game game;
    public GameRoom(UInt16 roomID)
    {
        RoomID = roomID;
        game = new Game();
    }

    public List<User> UserList = new();

    public ErrorCode EnterUser(User user)
    {
        if (UserList.Count >= 2)
        {
            return ErrorCode.FullRoom;
        }

        UserList.Add(user);

        if(UserList.Count == 2)
        {
            game.Start(UserList[0], UserList[1]);
            // TODO : 게임시작 패킷 보내주기.
        }

        return ErrorCode.None;
    }

    public void ExitUser(User user)
    {
        game.EndExitUser(user);
        UserList.Remove(user);
        // TODO : 게임 종료 처리 패킷 보내기
        // game.GetGameResult(); // 사용해서 보내기.

    }

    public List<User> GetUserList()
    {
        return UserList;
    }
}
