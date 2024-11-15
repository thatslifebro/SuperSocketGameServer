using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;

namespace GameServer;

public class RequestInfo : BinaryRequestInfo
{
    public Int16 PacketSize { get; private set; }
    public Int16 PacketID { get; private set; }
    public SByte Type { get; private set; }

    public RequestInfo(Int16 size, Int16 packetID, SByte type, byte[] body) : base(null, body)
    {
        PacketSize = size;
        PacketID = packetID;
        Type = type;
    }
}

public class RecvFilter : FixedHeaderReceiveFilter<RequestInfo>
{
    public RecvFilter() : base(PacketDef.HeaderSize)
    {
    }

    protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(header, offset, PacketDef.HeaderSize);
        }

        var packetSize = BitConverter.ToInt16(header, offset);
        var bodySize = packetSize - PacketDef.HeaderSize;
        return bodySize;
    }

    protected override RequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] buffer, int offset, int length)
    {
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(header.Array, 0, PacketDef.HeaderSize);

        return new RequestInfo(BitConverter.ToInt16(header.Array, 0),
                                       BitConverter.ToInt16(header.Array, 2),
                                       (SByte)header.Array[4],
                                       buffer.CloneRange(offset, length));
    }
}
