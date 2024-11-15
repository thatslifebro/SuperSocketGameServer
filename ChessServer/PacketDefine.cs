using MessagePack;

namespace GameServer;

public class PacketDef
{
    public const Int16 HeaderSize = 5;
}

public class PacketToBytes
{
    public static byte[] Make(EPacketID packetID, byte[] bodyData)
    {
        byte type = 0;
        var pktID = (Int16)packetID;

        Int16 bodyDataSize = 0;
        if (bodyData != null)
        {
            bodyDataSize = (Int16)bodyData.Length;
        }

        var packetSize = (Int16)(bodyDataSize + PacketDef.HeaderSize);

        var dataSource = new byte[packetSize];
        Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, dataSource, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(pktID), 0, dataSource, 2, 2);
        dataSource[4] = type;

        if (bodyData != null)
        {
            Buffer.BlockCopy(bodyData, 0, dataSource, 5, bodyDataSize);
        }

        return dataSource;
    }
    public static byte[] Make(EPacketID packetID)
    {
        Int16 packetSize = 5;
        Int16 pktID = (Int16)packetID;

        var dataSource = new byte[packetSize];
        Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, dataSource, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(pktID), 0, dataSource, 2, 2);
        dataSource[4] = 0;

        return dataSource;
    }


}

[MessagePackObject]
public class PKTReqLogin
{
    [Key(0)]
    public string UserID = "";
}

[MessagePackObject]
public class PKTResLogin
{
    [Key(0)]
    public ErrorCode Result;
}

[MessagePackObject]
public class PKTResGameRoomInfos
{
    [Key(0)]
    public ErrorCode Result;

    [Key(1)]
    public List<(UInt16, UInt16)> GameRoomInfos = new();
}

[MessagePackObject]
public class PKTResLeaveGameRoom
{
    [Key(0)]
    public ErrorCode Result;
}

[MessagePackObject]
public class PKTReqChat
{
    [Key(0)]
    public string Chat ="";
}

[MessagePackObject]
public class PKTNtfChat
{
    [Key(0)]
    public string Chat = "";
}

[MessagePackObject]
public class PKTReqEnterGameRoom
{
    [Key(0)]
    public ErrorCode Result;

    [Key(1)]
    public Int16 RoomID;
}

[MessagePackObject]
public class PKTResEnterGameRoom
{
    [Key(0)]
    public ErrorCode Result;
}

[MessagePackObject]
public class PKTNtfGameStart
{
    [Key(0)]
    public List<(string, string)> UserInfos = new();
}

[MessagePackObject]
public class PKTNtfGameEnd
{
    [Key(0)]
    public GameResult Result;
}

[MessagePackObject]
public class PKTReqMovePiece //todo : 규칙을 서버에서 하면 beforex,y, after xy만 받을듯.
{
    [Key(0)]
    public SByte LastMovedPiece;

    [Key(1)]
    public SByte BeforeMove_X;

    [Key(2)]
    public SByte BeforeMove_Y;

    [Key(3)]
    public SByte AfterMove_X;

    [Key(4)]
    public SByte AfterMove_Y;

    [Key(5)]
    public SByte Moves;

    [Key(6)]
    public SByte Castling;

    [Key(7)]
    public SByte Promotion;
}

[MessagePackObject]
public class PKTResMovePiece //todo : 규칙을 서버에서 하면 beforex,y, after xy만 받을듯.
{
    [Key(0)]
    public ErrorCode Result;
}

[MessagePackObject]
public class PKTNTFMovePiece //todo : 규칙을 서버에서 하면 beforex,y, after xy만 받을듯.
{
    [Key(0)]
    public SByte LastMovedPiece;

    [Key(1)]
    public SByte BeforeMove_X;

    [Key(2)]
    public SByte BeforeMove_Y;

    [Key(3)]
    public SByte AfterMove_X;

    [Key(4)]
    public SByte AfterMove_Y;

    [Key(5)]
    public SByte Moves;

    [Key(6)]
    public SByte Castling;

    [Key(7)]
    public SByte Promotion;
}