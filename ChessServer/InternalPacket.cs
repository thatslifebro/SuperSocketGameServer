namespace GameServer;

public class InternalPacket
{
    public string SessionID;
    public EPacketID PacketID;
    public byte[] BodyData;

    public InternalPacket(string sessionID, EPacketID packetID, byte[]? bodyData = null)
    {
        SessionID = sessionID;
        PacketID = packetID;
        BodyData = bodyData ?? [];
    }
}
