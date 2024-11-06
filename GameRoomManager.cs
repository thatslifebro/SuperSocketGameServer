using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            roomInfos.Add((room.RoomID, (UInt16)room.UserList.Count));
        }

        return roomInfos;
    }

    public List<GameRoom> GetRoomList() { return _roomList; }

    public void ExitGameRoom(User user)
    {
        var room = _roomList.Find(r => r.RoomID == user.RoomID);

        if(room == null)
        {
            Console.WriteLine($"Not found room: {user.RoomID}");
            return;
        }

        room.ExitUser(user);
    }
}
