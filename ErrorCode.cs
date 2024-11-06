using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public enum ErrorCode : Int16
{
    None = 0,
    AlreadyExistUser = 1001,
    FullRoom = 1002,

    AlreadyLoginUser = 2001,
    LoginBodySerializeError = 2002,
    NotLoginUser = 2003,
}
