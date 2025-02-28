using System;
using Unity.Networking.Transport;
using UnityEngine;

public enum OpCode
{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    REMATCH = 5,
    DISCONNECT = 6,
    ID = 7,
    NEW_BOARD = 8,
    CHAT = 9,
}

public static class NetUtility
{
    public static void OnData(DataStreamReader stream, NetworkConnection cnn, Server server = null)
    {
        NetMessage msg = null;
        var opCode = (OpCode)stream.ReadByte();
        switch (opCode)
        {
            case OpCode.KEEP_ALIVE: msg = new NetKeepAlive(stream); break;
            case OpCode.WELCOME: msg = new NetWelcome(stream); break;
            case OpCode.START_GAME: msg = new NetStartGame(stream); break;
            case OpCode.MAKE_MOVE: msg = new NetMakeMove(stream); break;
            case OpCode.REMATCH: msg = new NetRematch(stream); break;
            case OpCode.ID: msg = new NetID(stream); break;
            case OpCode.DISCONNECT: msg = new NetDisconnect(stream); break;
            case OpCode.NEW_BOARD: msg = new NetNewBoard(stream); break;
            case OpCode.CHAT: msg = new NetChat(stream); break;
            default:
                Debug.LogError("Message received has no opcode");
                break;
        }

        if (server != null)
            msg.ReceivedOnServer(cnn);
        else
            msg.ReceivedOnClient();
    }


    // Net messages
    public static Action<NetMessage> C_KEEP_ALIVE;
    public static Action<NetMessage> C_WELCOME;
    public static Action<NetMessage> C_START_GAME;
    public static Action<NetMessage> C_MAKE_MOVE;
    public static Action<NetMessage> C_REMATCH;
    public static Action<NetMessage> C_ID;
    // public static Action<NetMessage> C_DISCONNECT;
    public static Action<NetMessage> C_NEW_BOARD;
    public static Action<NetMessage> C_CHAT;
    public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
    public static Action<NetMessage, NetworkConnection> S_WELCOME;
    public static Action<NetMessage, NetworkConnection> S_START_GAME;
    public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;
    public static Action<NetMessage, NetworkConnection> S_REMATCH;
    public static Action<NetMessage, NetworkConnection> S_ID;
    // public static Action<NetMessage, NetworkConnection> S_DISCONNECT;
    public static Action<NetMessage, NetworkConnection> S_NEW_BOARD;
    public static Action<NetMessage, NetworkConnection> S_CHAT;
}
