﻿using System;
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
    PacketHandler _packetHandler = new();

    bool _isThreadRunning = false;

    Action<string, byte[]> SendData;

    public void Start(UserManager userManager, GameRoomManager gameRoomManager, Action<string, byte[]> networkSendFunc)
    {
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
        SendData = networkSendFunc;

        _packetHandler.Init(_userManager, _gameRoomManager, SendData);
        _packetHandler.RegisterHandler(_handlerMap);

        _isThreadRunning = true;
        _processThread = new Thread(this.Process);
        _processThread.Start();
    }

    public void Stop()
    {
        _isThreadRunning = false;
        _bufferBlock.Complete();
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

    
}
