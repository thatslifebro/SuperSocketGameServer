namespace GameServer;

public class GameRoomManager
{
    List<GameRoom> _roomList = new();

    public void CreateRooms()
    {
        for(UInt16 i = 0; i < MainServer._serverOption.RoomMaxCount; i++)
        {
            GameRoom room = new GameRoom(i);
            _roomList.Add(room);
        }
    }

    public List<(UInt16, UInt16)> GetGameRoomInfos()
    {
        List<(UInt16, UInt16)> roomInfos = new();

        foreach(var room in _roomList)
        {
            roomInfos.Add((room.RoomID, (UInt16)room.GameUserList.Count));
        }

        return roomInfos;
    }

    public GameRoom? GetRoom(Int16 roomId)
    {
        if(roomId < 0 || roomId >= MainServer._serverOption.RoomMaxCount)
        {
            return null;
        }
        return _roomList.Find(r => r.RoomID == roomId);
    }

    public void ExitGameRoom(User user)
    {
        var room = _roomList.Find(r => r.RoomID == user.RoomID);

        if(room == null)
        {
            //Console.WriteLine($"Not found room: {user.RoomID}");
            return;
        }

        room.ExitUser(user);
    }
}
