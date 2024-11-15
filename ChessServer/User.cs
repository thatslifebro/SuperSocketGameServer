namespace GameServer;

public class User
{
    public string SessionID;
    UInt32 SequenceID;
    public string UserID = string.Empty;

    public short RoomID { get; private set; } = -1;

    public User(string sessionID, string userId, UInt32 sequenceId)
    {
        SessionID = sessionID;
        UserID = userId;
        SequenceID = sequenceId;
    }

    public void EnterRoom(short roomID)
    {
        RoomID = roomID;
    }

    public void LeaveRoom()
    {
        RoomID = -1;
    }
}
