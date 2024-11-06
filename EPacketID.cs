using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public enum EPacketID : Int16
{
    SessionConnect = 1001,
    SessionDisconnect = 1002,
    SessionClose = 1003,

    ReqLogin = 2001,
    ResLogin = 2002,
    ReqGameRoomInfos = 2003,
    ResGameRoomInfos = 2004,
    ReqChat = 2005,
    NtfChat = 2006,

}
