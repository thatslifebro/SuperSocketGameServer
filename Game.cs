using MessagePack;
using Sakk_Alkalmazás_2._0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class Game
{
    public static Func<string, byte[], bool> NetSendFunc;

    public User? WhitePlayer;
    public User? BlackPlayer;

    GameStatus _gameStatus = GameStatus.None;
    GameResult _gameResult = GameResult.None;
    Turn _turn = Turn.White;

    public static Action<string, byte[]> SendData;

    public Game(Action<string, byte[]> sendData) { SendData = sendData; }

    // Game 규칙용

    #region Pieces
    BlackPawn blackPawn = new BlackPawn();
    BlackRook1 blackRook2 = new BlackRook1();
    BlackRook2 blackRook1 = new BlackRook2();
    BlackKnight blackKnight = new BlackKnight();
    BlackKnight2 blackKnight2 = new BlackKnight2();
    BlackBishop blackBishop = new BlackBishop();
    BlackBishop2 blackBishop2 = new BlackBishop2();
    BlackQueen blackQueen = new BlackQueen();
    BlackKing blackKing = new BlackKing();
    WhitePawn whitePawn = new WhitePawn();
    WhiteRook1 whiteRook2 = new WhiteRook1();
    WhiteRook2 whiteRook1 = new WhiteRook2();
    WhiteKnight whiteKnight = new WhiteKnight();
    WhiteKnight2 whiteKnight2 = new WhiteKnight2();
    WhiteBishop whiteBishop = new WhiteBishop();
    WhiteBishop2 whiteBishop2 = new WhiteBishop2();
    WhiteQueen whiteQueen = new WhiteQueen();
    WhiteKing whiteKing = new WhiteKing();
    #endregion

    #region bools
    public bool BlackRookMoved1 = true;
    public bool BlackRookMoved2 = true;
    public bool BlackKingMoved = true;
    public bool WhiteRookMoved1 = true;
    public bool WhiteRookMoved2 = true;
    public bool WhiteKingMoved = true;
    #endregion

    #region integers
    public int BeforeMove_I;
    public int BeforeMove_J;
    public int LastMovedPiece = 0;
    public int Moves = 0;
    public int Castling = 0;
    public int Promotionvalue = 0;
    public int PromotedPiece { get; set; }
    #endregion

    TableClass tableClass = new TableClass();
    public int[,] WhiteStaleArray = new int[8, 8]; // 갈수 있는 모든 곳
    public int[,] BlackStaleArray = new int[8, 8];

    public void Start(User blackPlayer, User whitePlayer)
    {
        _gameStatus = GameStatus.Playing;
        BlackPlayer = blackPlayer;
        WhitePlayer = whitePlayer;

        // 게임 초기화
        InitGame();

        // 패킷 전송
        NTFGameStart();
    }

    void InitGame()
    {
        tableClass.Table = new int[8, 8]
            {
                { 02, 03, 04, 05, 06, 09, 08, 07},
                { 01, 01, 01, 01, 01, 01, 01, 01},
                { 00, 00, 00, 00, 00, 00, 00, 00},
                { 00, 00, 00, 00, 00, 00, 00 ,00},
                { 00, 00, 00, 00, 00, 00, 00 ,00},
                { 00, 00, 00, 00, 00, 00, 00 ,00},
                { 11, 11, 11, 11, 11, 11, 11, 11},
                { 12, 13, 14, 15, 16, 19, 18, 17},
            };
        tableClass.PossibleMoves = new int[8, 8];
        tableClass.AllPossibleMoves = new int[8, 8];
        GetPiecesOnBoard(); // 기물이 있는곳 1로 채우기
        StaleArrays(); // 갈수 있는곳 2로 채우기
    }

    public void GetPiecesOnBoard()
    {
        int i, j;
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                if (tableClass.Table[i, j] != 0)
                {
                    tableClass.PossibleMoves[i, j] = 1;
                }
                else
                {
                    tableClass.PossibleMoves[i, j] = 0;
                }
            }
        }
    }

    public void StaleArrays()
    {
        //its very simple, we save every possible moves by players
        WhiteStaleArray = new int[8, 8];
        WhiteStaleArray = blackPawn.IsStale(tableClass.Table, WhiteStaleArray);
        WhiteStaleArray = blackRook1.IsStale(tableClass.Table, WhiteStaleArray);
        WhiteStaleArray = blackRook2.IsStale(tableClass.Table, WhiteStaleArray);
        WhiteStaleArray = blackKnight.IsStale(tableClass.Table, WhiteStaleArray);
        WhiteStaleArray = blackKnight2.IsStale(tableClass.Table, WhiteStaleArray);
        WhiteStaleArray = blackBishop.IsStale(tableClass.Table, WhiteStaleArray);
        WhiteStaleArray = blackBishop2.IsStale(tableClass.Table, WhiteStaleArray);
        WhiteStaleArray = blackQueen.IsStale(tableClass.Table, WhiteStaleArray);

        BlackStaleArray = new int[8, 8];
        BlackStaleArray = whitePawn.IsStale(tableClass.Table, BlackStaleArray);
        BlackStaleArray = whiteRook1.IsStale(tableClass.Table, BlackStaleArray);
        BlackStaleArray = whiteRook2.IsStale(tableClass.Table, BlackStaleArray);
        BlackStaleArray = whiteKnight.IsStale(tableClass.Table, BlackStaleArray);
        BlackStaleArray = whiteKnight2.IsStale(tableClass.Table, BlackStaleArray);
        BlackStaleArray = whiteBishop.IsStale(tableClass.Table, BlackStaleArray);
        BlackStaleArray = whiteBishop2.IsStale(tableClass.Table, BlackStaleArray);
        BlackStaleArray = whiteQueen.IsStale(tableClass.Table, BlackStaleArray);

    }

    public void End(GameResult gameResult)
    {
        _gameStatus = GameStatus.End;
        _gameResult = gameResult;
        _turn = Turn.White;
    }

    public void EndExitUser(User exitUser)
    {
        if (exitUser == WhitePlayer)
        {
            End(GameResult.BlackWin);
        }
        else if (exitUser == BlackPlayer)
        {
            End(GameResult.WhiteWin);
        }

        // TODO : 게임 종료 패킷 전송
    }

    public GameResult GetGameResult()
    {
        return _gameResult;
    }

    public void NTFGameStart()
    {
        var packet = new PKTNtfGameStart();
        var UserInfos = new List<(string, string)>();
        UserInfos.Add((WhitePlayer.SessionID, WhitePlayer.UserID));
        UserInfos.Add((BlackPlayer.SessionID, BlackPlayer.UserID));

        packet.UserInfos = UserInfos;

        var bodyData = MessagePackSerializer.Serialize(packet);
        var sendPacket = PacketToBytes.Make(EPacketID.NTFGameStart, bodyData);

        SendData(BlackPlayer.SessionID, sendPacket);
        SendData(WhitePlayer.SessionID, sendPacket);
    }

    public void MovePiece(string sessionID, PKTReqMovePiece request)
    {
        ErrorCode errorCode = ErrorCode.None;
        var opponentID = sessionID == WhitePlayer.SessionID ? BlackPlayer.SessionID : WhitePlayer.SessionID;

        if (_gameStatus != GameStatus.Playing)
        {
            errorCode = ErrorCode.InvalidGameStatus;
        }

        if (_turn == Turn.Black && BlackPlayer.SessionID != sessionID)
        {
            errorCode = ErrorCode.NotYourTurn;
        }

        if (_turn == Turn.White && WhitePlayer.SessionID != sessionID)
        {
            errorCode = ErrorCode.NotYourTurn;
        }

        // 선택한 기물이 움직일 수 있는 곳인지 확인.

        BeforeMove_I = request.BeforeMove_X;
        BeforeMove_J = request.BeforeMove_Y;
        int ax = request.AfterMove_X;
        int ay = request.AfterMove_Y;

        if (tableClass.PossibleMoves[BeforeMove_I, BeforeMove_J] != 0)
        {
            // 기물이 없음 에러
        }

        // 해당 기물이 갈 수 있는 곳을 표시. Possiblemoves에 2로 표시
        var player = sessionID == WhitePlayer.SessionID ? WhitePlayer : BlackPlayer;
        var otherPlayerTurn = ((player == WhitePlayer) && (_turn == Turn.Black)) || ((player == BlackPlayer) && (_turn == Turn.White)) ? true : false;
        PossibleMovesByPieces(tableClass.Table[BeforeMove_I, BeforeMove_J], BeforeMove_I, BeforeMove_J, otherPlayerTurn);

        // 이동할 수 없는 곳이면 에러
        if (tableClass.PossibleMoves[ax, ay] != 2)
        {
            // 이동할 수 없음 에러
        }

        // 캐슬링 조건확인 및 프로모션 체크하기.
        CastlingAndPawnPromotionChecker(ax, ay, (int)request.Promotion, player);

        // TODO : 캐슬링 체크

        // 이동
        tableClass.Table[ax, ay] = tableClass.Table[BeforeMove_I, BeforeMove_J];
        tableClass.Table[BeforeMove_I, BeforeMove_J] = 0;
        
        // 정리
        StaleArrays();
        EndMove();
        EveryPossibleMoves();

        // 패킷 전송
        var response = new PKTResMovePiece() { Result = errorCode };
        var bodyData = MessagePackSerializer.Serialize(response);
        var sendPacket = PacketToBytes.Make(EPacketID.ResMovePiece, bodyData);

        SendData(sessionID, sendPacket);

        // 상대에게 전송
        if (errorCode == ErrorCode.None)
        {
            _turn = _turn == Turn.White ? Turn.Black : Turn.White;

            var bodyData2 = MessagePackSerializer.Serialize(request);
            var sendPacket2 = PacketToBytes.Make(EPacketID.NtfMovePiece, bodyData2);

            SendData(opponentID, sendPacket2);
        }

        // 게임 종료여부 파악
        CheckMateChecker();

        // 게임 종료 패킷 보내기.
        //if (_gameStatus == GameStatus.End)
        //{
        //    var packet = new PKTNTFGameEnd() { Result = _gameResult };
        //    var bodyData3 = MessagePackSerializer.Serialize(packet);
        //    var sendPacket3 = PacketToBytes.Make(EPacketID.NTFGameEnd, bodyData3);

        //    SendData(BlackPlayer.SessionID, sendPacket3);
        //    SendData(WhitePlayer.SessionID, sendPacket3);
        //}
        
    }

    public void CheckMateChecker()
    {
        if (Moves == 0)
        {
            _gameStatus = GameStatus.End;
            _gameResult = _turn != Turn.White ? GameResult.BlackWin : GameResult.WhiteWin;
        }
    }


    public void EndMove()
    {
        int i, j;
        //we check cells and if there are pieces that position will get value 1 in the PossibleMoves array
        //its important because you can only click that cells where the value is 1
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                if (tableClass.Table[i, j] != 0)
                {
                    tableClass.PossibleMoves[i, j] = 1;
                }
                else
                {
                    tableClass.PossibleMoves[i, j] = 0;
                }
            }
        }
    }

     public void PossibleMovesByPieces(int x, int i, int j, bool OtherPlayerTurn)
    {
        var WhiteTurn = _turn == Turn.White ? true : false;

        //here with the switch we will add the right move positions to our PossibleMoves array as value 2, with use of our classes
        switch (x)
        {
            case 1:
                tableClass.PossibleMoves = blackPawn.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 2:
                tableClass.PossibleMoves = blackRook1.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 3:
                tableClass.PossibleMoves = blackKnight.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 4:
                tableClass.PossibleMoves = blackBishop.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 5:
                tableClass.PossibleMoves = blackQueen.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 6:
                tableClass.PossibleMoves = blackKing.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, BlackKingMoved, BlackRookMoved1, BlackRookMoved2, OtherPlayerTurn);
                break;
            case 7:
                tableClass.PossibleMoves = blackRook2.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 8:
                tableClass.PossibleMoves = blackKnight.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 9:
                tableClass.PossibleMoves = blackBishop.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 11:
                tableClass.PossibleMoves = whitePawn.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 12:
                tableClass.PossibleMoves = whiteRook1.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 13:
                tableClass.PossibleMoves = whiteKnight.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 14:
                tableClass.PossibleMoves = whiteBishop.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 15:
                tableClass.PossibleMoves = whiteQueen.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 16:
                tableClass.PossibleMoves = whiteKing.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, WhiteKingMoved, WhiteRookMoved1, WhiteRookMoved2, OtherPlayerTurn);
                break;
            case 17:
                tableClass.PossibleMoves = whiteRook2.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 18:
                tableClass.PossibleMoves = whiteKnight2.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
            case 19:
                tableClass.PossibleMoves = whiteBishop.GetPossibleMoves(tableClass.Table, tableClass.PossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                break;
        }
        //the position of the selected piece will get value 3
        tableClass.PossibleMoves[i, j] = 3;
        //we should not enable impossible moves, like let our king in danger, so we have to validate moves
        RemoveMoveThatNotPossible(x, i, j);
    }

    public void RemoveMoveThatNotPossible(int x, int a, int b)
    {
        var WhiteTurn = _turn == Turn.White ? true : false;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                //we will check every available moves, so every time when the array is 2
                if (tableClass.PossibleMoves[i, j] == 2)
                {
                    /*we have to save the original position, because we will simulate that what will happen in every available move position and then
                     we need to restore it*/
                    int SelectedPiece = tableClass.Table[i, j];
                    //here we simulate the new position
                    tableClass.Table[i, j] = x;
                    tableClass.Table[a, b] = 0;
                    //we have 2 arrays with every possible stale situation, and with this method calling we refresh it with our new simulation positions
                    StaleArrays();
                    //this two condition will check that the opponents stale array contains the positon of the king
                    //if theres invalid moves then one of them statement will be true, and then it delete that moves
                    if (tableClass.NotValidMoveChecker(tableClass.Table, WhiteStaleArray, BlackStaleArray) == 1 && WhiteTurn)
                    {
                        tableClass.PossibleMoves[i, j] = 0;
                        if (i == 7 && j == 3 && x == 16)
                        {
                            tableClass.PossibleMoves[7, 2] = 0;
                        }
                    }
                    if (tableClass.NotValidMoveChecker(tableClass.Table, WhiteStaleArray, BlackStaleArray) == 2 && !WhiteTurn)
                    {
                        tableClass.PossibleMoves[i, j] = 0;
                        if (i == 0 && j == 3 && x == 6)
                        {
                            tableClass.PossibleMoves[0, 2] = 0;
                        }
                    }
                    //at here we restore everything
                    tableClass.Table[i, j] = SelectedPiece;
                    tableClass.Table[a, b] = x;
                    StaleArrays();
                }
            }
        }
    }

    public void EveryPossibleMoves()
    {
        var WhiteTurn = _turn == Turn.White ? true : false;
        var OtherPlayerTurn = ((WhiteTurn && _turn == Turn.Black) || (!WhiteTurn && _turn == Turn.White)) ? true : false;

        int i = 0;
        int j = 0;
        //we should reset the array after every move
        tableClass.AllPossibleMoves = new int[8, 8];
        //this need because we need that player who isn't turned yet
        WhiteTurn = !WhiteTurn;
        //pieces
        for (int x = 1; x < 20; x++)
        {
            //and positions
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if (tableClass.Table[i, j] == x)
                    {
                        //similar like earlier switchs
                        switch (x)
                        {
                            case 1:
                                tableClass.AllPossibleMoves = blackPawn.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 2:
                                tableClass.AllPossibleMoves = blackRook1.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 3:
                                tableClass.AllPossibleMoves = blackKnight.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 4:
                                tableClass.AllPossibleMoves = blackBishop.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 5:
                                tableClass.AllPossibleMoves = blackQueen.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 6:
                                tableClass.AllPossibleMoves = blackKing.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, BlackKingMoved, BlackRookMoved1, BlackRookMoved2, OtherPlayerTurn);
                                break;
                            case 7:
                                tableClass.AllPossibleMoves = blackRook2.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 8:
                                tableClass.AllPossibleMoves = blackKnight.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 9:
                                tableClass.AllPossibleMoves = blackBishop2.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 11:
                                tableClass.AllPossibleMoves = whitePawn.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 12:
                                tableClass.AllPossibleMoves = whiteRook1.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 13:
                                tableClass.AllPossibleMoves = whiteKnight.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 14:
                                tableClass.AllPossibleMoves = whiteBishop.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 15:
                                tableClass.AllPossibleMoves = whiteQueen.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 16:
                                tableClass.AllPossibleMoves = whiteKing.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, WhiteKingMoved, WhiteRookMoved1, WhiteRookMoved2, OtherPlayerTurn);
                                break;
                            case 17:
                                tableClass.AllPossibleMoves = whiteRook2.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 18:
                                tableClass.AllPossibleMoves = whiteKnight2.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                            case 19:
                                tableClass.AllPossibleMoves = whiteBishop2.GetPossibleMoves(tableClass.Table, tableClass.AllPossibleMoves, i, j, WhiteTurn, OtherPlayerTurn);
                                break;
                        }
                        //okay we got moves by pieces, now we should delete that are invalids
                        RemoveMoveThatNotPossible2(x, i, j);
                    }
                }
            }
        }
        //and now we change back it
        WhiteTurn = !WhiteTurn;
    }

    public void RemoveMoveThatNotPossible2(int x, int a, int b)
    {
        var WhiteTurn = _turn == Turn.White ? true : false;

        //you already know this
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                //you can remember that value 2 is the possible moves
                if (tableClass.AllPossibleMoves[i, j] == 2)
                {
                    //and this is very familiar than the "RemoveMoveThatNotPossible", lets simulate
                    int lastHitPiece = tableClass.Table[i, j];

                    tableClass.Table[i, j] = x;
                    tableClass.Table[a, b] = 0;
                    StaleArrays();
                    //invalid move
                    if (tableClass.NotValidMoveChecker(tableClass.Table, WhiteStaleArray, BlackStaleArray) == 1 && WhiteTurn)
                    {
                        tableClass.AllPossibleMoves[i, j] = 0;
                    }
                    //invalid move
                    if (tableClass.NotValidMoveChecker(tableClass.Table, WhiteStaleArray, BlackStaleArray) == 2 && !WhiteTurn)
                    {
                        tableClass.AllPossibleMoves[i, j] = 0;
                    }
                    //this is a valid move, so we increment the Moves integer
                    //if Moves not equals to 0, than its not checkmate
                    if (tableClass.NotValidMoveChecker(tableClass.Table, WhiteStaleArray, BlackStaleArray) == 3)
                    {
                        Moves++;
                    }
                    tableClass.Table[i, j] = lastHitPiece;
                    tableClass.Table[a, b] = x;
                    StaleArrays();
                }
            }
        }
        tableClass.AllPossibleMoves = new int[8, 8];
    }

    public void CastlingAndPawnPromotionChecker(int i, int j, int promotionValue, User user)
    {
        //if one of the player moved with his rook or king then they can not make castling anymore
        //we check this in this switch
        switch (tableClass.Table[BeforeMove_I, BeforeMove_J])
        {
            case 02:
                BlackRookMoved1 = false;
                break;
            case 07:
                BlackRookMoved2 = false;
                break;
            case 12:
                WhiteRookMoved1 = false;
                break;
            case 17:
                WhiteRookMoved2 = false;
                break;
            case 06:
                BlackKingMoved = false;
                break;
            case 16:
                WhiteKingMoved = false;
                break;
        }

        if (tableClass.Table[BeforeMove_I, BeforeMove_J] == 01)
        {
            if (i == 7)  // black
            {
                if(user == BlackPlayer)
                {
                    int[] possibleValue = { 05, 04, 03, 02 };
                    if (possibleValue.Contains(promotionValue))
                    {
                        tableClass.Table[BeforeMove_I, BeforeMove_J] = promotionValue;
                        Promotionvalue = promotionValue;
                    }
                    else
                    {
                        // 오류
                    }
                }
                else
                {
                    // 오류
                }
            }
        }
        if (tableClass.Table[BeforeMove_I, BeforeMove_J] == 11)
        {
            if (i == 0)
            {
                if (user == WhitePlayer)
                {
                    int[] possibleValue = { 15, 14, 13, 12 };
                    if (possibleValue.Contains(promotionValue))
                    {
                        tableClass.Table[BeforeMove_I, BeforeMove_J] = promotionValue;
                        Promotionvalue = promotionValue;
                    }
                    else
                    {
                        //오류
                    }
                }
                else
                {
                    // 오류
                }
            }
        }
    }
}
