using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
