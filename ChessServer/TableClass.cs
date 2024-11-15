namespace GameServer;

class TableClass
{
    public int[,] Table { get; set; } = new int[8, 8];
    public int[,] PossibleMoves { get; set; } = new int[8, 8];
    public int[,] AllPossibleMoves { get; set; } = new int[8, 8];
    public bool WhiteStaleUp = false;
    public bool BlackStaleUp = false;
    public bool CancelLastMove = false;

    public int NotValidMoveChecker(int[,] Table, int[,] WhiteStaleArray, int[,] BlackStaleArray)
    {
        int WhiteKingPositionI = 0;
        int WhiteKingPositionJ = 0;
        int BlackKingPositionI = 0;
        int BlackKingPositionJ = 0;

        for (int a = 0; a < 8; a++)
        {
            for (int b = 0; b < 8; b++)
            {
                if (Table[a, b] == 16)
                {
                    WhiteKingPositionI = a;
                    WhiteKingPositionJ = b;
                }
                if (Table[a, b] == 06)
                {
                    BlackKingPositionI = a;
                    BlackKingPositionJ = b;
                }
            }
        }
        if (WhiteStaleArray[WhiteKingPositionI, WhiteKingPositionJ] == 2)
        {
            return 1;
        }
        if (BlackStaleArray[BlackKingPositionI, BlackKingPositionJ] == 2)
        {
            return 2;
        }
        return 3;

    }
}
