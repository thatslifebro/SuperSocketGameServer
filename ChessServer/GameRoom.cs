namespace GameServer;

public class GameRoom
{
    public UInt16 RoomID { get; private set; }
    public Game game;

    public static Action<string, byte[]>? SendData;

    public GameRoom(UInt16 roomID)
    {
        RoomID = roomID;
        game = new Game(SendData!);
    }

    public List<User> GameUserList = new();

    public ErrorCode EnterUser(User user)
    {
        if (GameUserList.Count >= 2)
        {
            return ErrorCode.FullRoom;
        }

        GameUserList.Add(user);

        if(GameUserList.Count == 2)
        {
            game.Start(GameUserList[0], GameUserList[1]);
        }

        user.EnterRoom((short)RoomID);

        return ErrorCode.None;
    }

    public void ExitUser(User user)
    {
        game.EndExitUser(user);
        user.LeaveRoom();
        GameUserList.Remove(user);
        // TODO : 게임 종료 처리 패킷 보내기
        // game.GetGameResult(); // 사용해서 보내기.



    }

    public List<User> GetUserList()
    {
        return GameUserList;
    }
}
