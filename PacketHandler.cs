using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

public class PacketHandler
{
    UserManager _userManager;
    GameRoomManager _gameRoomManager;
    Action<string, byte[]> SendData;

    public void Init(UserManager userManager, GameRoomManager gameRoomManager, Action<string, byte[]> sendData)
    {
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
        SendData = sendData;
    }

    public void RegisterHandler(Dictionary<EPacketID, Action<InternalPacket>> handlerMap)
    {
        handlerMap.Add(EPacketID.SessionConnect, OnSessionConnect);
        handlerMap.Add(EPacketID.SessionClose, OnSessionClosed);
        handlerMap.Add(EPacketID.ReqLogin, ReqLoginHandler);
        handlerMap.Add(EPacketID.ReqGameRoomInfos, ReqGameRoomInfosHandler);
        handlerMap.Add(EPacketID.ReqChat, ReqChatHandler);
        handlerMap.Add(EPacketID.ReqEnterGameRoom, ReqEnterGameRoomHandler);
        handlerMap.Add(EPacketID.ReqMovePiece, ReqMovePieceHandler);
        handlerMap.Add(EPacketID.ReqLeaveGameRoom, ReqLeaveGameRoomHandler);
    }

    public void OnSessionConnect(InternalPacket internalPacket)
    {
    }

    public void OnSessionClosed(InternalPacket internalPacket)
    {
        var sessionID = internalPacket.SessionID;

        var user = _userManager.GetUser(sessionID);
        if (user == null)
        {
            Console.WriteLine($"[OnSessionClosed] Invalid SessionID: {sessionID}");
            return;
        }

        _gameRoomManager.ExitGameRoom(user);

        _userManager.RemoveUser(sessionID);
    }

    public void ReqLoginHandler(InternalPacket internalPacket)
    {
        var sessionID = internalPacket.SessionID;

        if (_userManager.GetUser(sessionID) != null)
        {
            SendLoginResponseToClient(sessionID, ErrorCode.AlreadyLoginUser);
            return;
        }

        var bodyData = internalPacket.BodyData;
        var request = MessagePackSerializer.Deserialize<PKTReqLogin>(bodyData);
        if (request == null)
        {
            SendLoginResponseToClient(sessionID, ErrorCode.LoginBodySerializeError);
            return;
        }

        var errorCode = _userManager.AddUser(sessionID, request.UserID);
        if (errorCode != ErrorCode.None)
        {
            SendLoginResponseToClient(sessionID, errorCode);
            return;
        }

        SendLoginResponseToClient(sessionID, ErrorCode.None);
    }

    public void SendLoginResponseToClient(string sessionID, ErrorCode errorCode)
    {
        var response = new PKTResLogin() { Result = errorCode };

        var bodyData = MessagePackSerializer.Serialize(response);

        var packet = PacketToBytes.Make(EPacketID.ResLogin, bodyData);

        SendData(sessionID, packet);

        Console.WriteLine($"SendLoginResponseToClient: {errorCode}");
    }

    public void ReqGameRoomInfosHandler(InternalPacket internalPacket)
    {
        var sessionID = internalPacket.SessionID;

        if (_userManager.GetUser(sessionID) == null)
        {
            SendGameRoomInfosResponseToClient(sessionID, ErrorCode.NotLoginUser);
            return;
        }

        SendGameRoomInfosNtf();
    }

    public void SendGameRoomInfosNtf()
    {
        var response = new PKTResGameRoomInfos() { Result = ErrorCode.None };

        response.GameRoomInfos = _gameRoomManager.GetGameRoomInfos();

        var bodyData = MessagePackSerializer.Serialize(response);

        var packet = PacketToBytes.Make(EPacketID.ResGameRoomInfos, bodyData);

        _userManager.GetAllUser().ForEach(user => SendData(user.SessionID, packet));
    }

    public void SendGameRoomInfosResponseToClient(string sessionID, ErrorCode errorCode)
    {
        var response = new PKTResGameRoomInfos() { Result = errorCode };

        if (errorCode == ErrorCode.None)
        {
            response.GameRoomInfos = _gameRoomManager.GetGameRoomInfos();
        }

        var bodyData = MessagePackSerializer.Serialize(response);

        var packet = PacketToBytes.Make(EPacketID.ResGameRoomInfos, bodyData);

        SendData(sessionID, packet);

        //Console.WriteLine($"SendGameRoomInfosResponseToClient: {errorCode}");
    }

    public void ReqChatHandler(InternalPacket internalPacket)
    {
        var sessionID = internalPacket.SessionID;

        var user = _userManager.GetUser(sessionID);
        if (user == null)
        {
            return;
        }

        var bodyData = internalPacket.BodyData;
        var request = MessagePackSerializer.Deserialize<PKTReqChat>(bodyData);
        if (request == null)
        {
            return;
        }

        request.Chat = $"{user.UserID} : {request.Chat}";

        SendChatNtfToAll(request.Chat);
    }

    public void SendChatNtfToAll(string chat)
    {
        var response = new PKTNtfChat() { Chat = chat };

        var bodyData = MessagePackSerializer.Serialize(response);

        var packet = PacketToBytes.Make(EPacketID.NtfChat, bodyData);

        _userManager.GetAllUser().ForEach(user => SendData(user.SessionID, packet));

        //Console.WriteLine($"SendGameRoomInfosResponseToClient: {errorCode}");
    }

    public void ReqEnterGameRoomHandler(InternalPacket internalPacket)
    {
        var sessionID = internalPacket.SessionID;

        var user = _userManager.GetUser(sessionID);
        if (user == null)
        {
            SendEnterGameRoomResponseToClient(sessionID, ErrorCode.NotLoginUser);
            return;
        }

        var bodyData = internalPacket.BodyData;
        var request = MessagePackSerializer.Deserialize<PKTReqEnterGameRoom>(bodyData);
        if (request == null)
        {
            SendEnterGameRoomResponseToClient(sessionID, ErrorCode.BodyDataError);
            return;
        }

        var room = _gameRoomManager.GetRoom(request.RoomID);
        if (room == null)
        {
            SendEnterGameRoomResponseToClient(sessionID, ErrorCode.InvalidGameRoomID);
            return;
        }

        var errorCode = room.EnterUser(user);
        if (errorCode != ErrorCode.None)
        {
            SendEnterGameRoomResponseToClient(sessionID, errorCode);
            return;
        }

        SendEnterGameRoomResponseToClient(sessionID, ErrorCode.None);

        SendGameRoomInfosNtf();
    }

    public void ReqLeaveGameRoomHandler(InternalPacket internalPacket)
    {
        var sessionID = internalPacket.SessionID;

        var user = _userManager.GetUser(sessionID);
        if (user == null)
        {
            return;
        }

        var room = _gameRoomManager.GetRoom(user.RoomID);
        if (room == null)
        {
            return;
        }

        room.ExitUser(user);

        SendLeaveGameRoomToClient(sessionID, ErrorCode.None);

        SendGameRoomInfosNtf();
    }

    public void SendLeaveGameRoomToClient(string sessionID, ErrorCode errorCode)
    {
        var response = new PKTResLeaveGameRoom() { Result = errorCode };

        var bodyData = MessagePackSerializer.Serialize(response);

        var packet = PacketToBytes.Make(EPacketID.ResLeaveGameRoom, bodyData);

        SendData(sessionID, packet);

        //Console.WriteLine($"SendGameRoomInfosResponseToClient: {errorCode}");
    }

    public void SendEnterGameRoomResponseToClient(string sessionID, ErrorCode errorCode)
    {
        var response = new PKTReqEnterGameRoom() { Result = errorCode };

        var bodyData = MessagePackSerializer.Serialize(response);

        var packet = PacketToBytes.Make(EPacketID.ResEnterGameRoom, bodyData);

        SendData(sessionID, packet);

        //Console.WriteLine($"SendGameRoomInfosResponseToClient: {errorCode}");
    }

    public void ReqMovePieceHandler(InternalPacket internalPacket)
    {
        var sessionID = internalPacket.SessionID;

        var user = _userManager.GetUser(sessionID);
        if (user == null)
        {
            return;
        }

        var bodyData = internalPacket.BodyData;
        var request = MessagePackSerializer.Deserialize<PKTReqMovePiece>(bodyData);
        if (request == null)
        {
            return;
        }

        var room = _gameRoomManager.GetRoom(user.RoomID);
        if (room == null)
        {
            return;
        }

        var game = room.game;

        game.MovePiece(sessionID, request);
    }
}
