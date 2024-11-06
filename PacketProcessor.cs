using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MessagePack;


namespace GameServer;

public class PacketProcessor
{
    BufferBlock<InternalPacket> _bufferBlock = new();
    Thread _processThread = null;

    Dictionary<EPacketID, Action<InternalPacket>> _handlerMap = new();

    UserManager _userManager = new();
    GameRoomManager _gameRoomManager = new();

    bool _isThreadRunning = false;

    Action<string, byte[]> SendData;

    public void Start(UserManager userManager, GameRoomManager gameRoomManager, Action<string, byte[]> networkSendFunc)
    {
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
        SendData = networkSendFunc;

        RegisterHandler();

        _isThreadRunning = true;
        _processThread = new Thread(this.Process);
        _processThread.Start();
    }

    public void Stop()
    {
        _isThreadRunning = false;
        _bufferBlock.Complete();
    }

    public void RegisterHandler()
    {
        _handlerMap.Add(EPacketID.SessionConnect, OnSessionConnect);
        _handlerMap.Add(EPacketID.SessionDisconnect, OnSessionDisconnect);
        _handlerMap.Add(EPacketID.ReqLogin, ReqLoginHandler);
        _handlerMap.Add(EPacketID.ReqGameRoomInfos, ReqGameRoomInfosHandler);
        _handlerMap.Add(EPacketID.ReqChat, ReqChatHandler);
    }

    public void Process()
    {
        try
        {
            while (_isThreadRunning)
            {
                var internalPacket = _bufferBlock.Receive();

                if (_handlerMap.TryGetValue(internalPacket.PacketID, out var handler))
                {
                    handler(internalPacket);
                }
                else
                {
                    Console.WriteLine($"[Process] Invalid PacketID: {internalPacket.PacketID}");
                }
            }
        }
        catch (Exception ex)
        {
            _isThreadRunning.IfTrue(() => Console.WriteLine($"[Process] Exception: {ex.ToString()}"));
        }
    }

    public void InsertPacket(InternalPacket internalPacket)
    {
        _bufferBlock.Post(internalPacket);
    }

    public void OnSessionConnect(InternalPacket internalPacket)
    {
    }

    public void OnSessionDisconnect(InternalPacket internalPacket)
    {
        var sessionID = internalPacket.SessionID;

        var user = _userManager.GetUser(sessionID);
        if(user == null)
        {
            Console.WriteLine($"[OnSessionDisconnect] Invalid SessionID: {sessionID}");
            return;
        }

        _gameRoomManager.ExitGameRoom(user);

        _userManager.RemoveUser(sessionID);
    }

    public void ReqLoginHandler(InternalPacket internalPacket)
    {
        var sessionID = internalPacket.SessionID;

        if(_userManager.GetUser(sessionID) != null)
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
        if(errorCode != ErrorCode.None)
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

        SendGameRoomInfosResponseToClient(sessionID, ErrorCode.None);
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
}
