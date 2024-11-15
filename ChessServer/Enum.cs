using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public enum GameStatus : Int16
{
    None = 0,
    Playing = 1,
    End = 2,
}

public enum GameResult : Int16
{
    None = 0,
    WhiteWin = 1,
    BlackWin = 2,
    Draw = 3,
}

public enum Turn : Int16
{
    Black = 0,
    White = 1,
}
