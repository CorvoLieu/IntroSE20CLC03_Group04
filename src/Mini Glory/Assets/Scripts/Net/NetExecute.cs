using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using UnityEngine.SceneManagement;
using System;

public class NetExecute : MonoBehaviour
{
    public Server server;
    public Client client;


    // multi logic
    private int playerCount = -1;
    public static int currentTeam = -1;
    private bool localGame = true;
    private bool[] playerRematch = new bool[2];
    [HideInInspector] public int[] playerID = new int[2];



    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        RegisterEvents();
    }


    #region
    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_REMATCH += OnRematchServer;
        NetUtility.S_ID += OnIDServer;
        // NetUtility.S_DISCONNECT += OnDisconnectServer;
        NetUtility.S_NEW_BOARD += OnNewBoardServer;
        NetUtility.S_CHAT += OnChatServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_REMATCH += OnRematchClient;
        // NetUtility.C_DISCONNECT += OnDisconnectClient;
        NetUtility.C_NEW_BOARD += OnNewBoardClient;
        NetUtility.C_CHAT += OnChatClient;
    }

    private void UnRegisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_REMATCH -= OnRematchServer;
        NetUtility.S_ID -= OnIDServer;
        // NetUtility.S_DISCONNECT -= OnDisconnectServer;
        NetUtility.S_NEW_BOARD -= OnNewBoardServer;
        NetUtility.S_CHAT -= OnChatServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
        NetUtility.C_REMATCH -= OnRematchClient;
        // NetUtility.C_DISCONNECT -= OnDisconnectClient;
        NetUtility.C_NEW_BOARD -= OnNewBoardClient;
        NetUtility.C_CHAT -= OnChatClient;
    }





    // Server
    private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn)
    {
        // Debug.Log("[SERVER] Receive make move");
        NetMakeMove mm = msg as NetMakeMove;
        if (Server.Instance.connections[0] == cnn)
            Server.Instance.SendToClient(Server.Instance.connections[1], mm);
        else
            Server.Instance.SendToClient(Server.Instance.connections[0], mm);
    }
    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        NetWelcome nw = msg as NetWelcome;
        nw.AssignedTeam = ++playerCount;
        Server.Instance.SendToClient(cnn, nw);

        // if there are enough 2 connections, start the game
        if (playerCount == 1)
            Server.Instance.Broadcast(new NetStartGame());
    }
    private void OnRematchServer(NetMessage msg, NetworkConnection cnn)
    {
        Server.Instance.Broadcast(msg);
    }
    private void OnIDServer(NetMessage msg, NetworkConnection cnn)
    {
        NetID ni = msg as NetID;
        // Debug.Log("[SERVER] Receive ID " + ni.PlayerID.ToString());
        Server.Instance.Broadcast(msg);

        playerID[playerCount] = ni.PlayerID;
        // Debug.Log($"[SERVER] Player ID {playerID[playerCount]} saved as player {playerCount}");
    }
    private void OnDisconnectServer(NetMessage msg, NetworkConnection cnn)
    {
        Debug.Log("[SERVER] Shutting down server");
        Server.Instance.Broadcast(msg as NetDisconnect);
        Server.Instance.Shutdown();
    }

    private void OnNewBoardServer(NetMessage msg, NetworkConnection cnn)
    {
        // Debug.Log("[SERVER] Broadcasting new board command");
        var nn = msg as NetNewBoard;
        Server.Instance.Broadcast(nn);
    }
    private void OnChatServer(NetMessage msg, NetworkConnection cnn)
    {
        NetChat nc = msg as NetChat;
        // Debug.Log($"[SERVER] Receive chat msg: {nc.msg}");
        Server.Instance.Broadcast(nc);
    }





    // Client
    private void OnWelcomeClient(NetMessage msg)
    {
        NetWelcome nw = msg as NetWelcome;
        currentTeam = nw.AssignedTeam;

        Debug.Log($"[CLIENT] My assigned team is {nw.AssignedTeam}");
    }
    private void OnStartGameClient(NetMessage msg)
    {
        // We just need to go to the game scene
        // gameObject.SetActive(false);
        GameController.EnterGame(currentTeam);
    }
    private void OnRematchClient(NetMessage msg)
    {
        NetRematch rm = msg as NetRematch;

        playerRematch[rm.teamId] = rm.wantRematch == 1;

        // activate the piece of UI

        // if both players want to rematch
    }
    private void OnMakeMoveClient(NetMessage msg)
    {
        NetMakeMove mm = msg as NetMakeMove;

        // Debug.Log($"MM: {mm.originalX} {mm.originalY} -> {mm.destinationX} {mm.destinationY}");

        GameController.updatePos(mm.originalX, mm.originalY, mm.destinationX, mm.destinationY);
    }
    private void OnDisconnectClient(NetMessage msg)
    {
        Debug.Log("[CLIENT] Shutting down client");
        Client.Instance.Shutdown();
        SceneManager.LoadScene(5);
    }
    private void OnNewBoardClient(NetMessage msg)
    {
        Debug.Log("[CLIENT] Receive New Board command");

        var nb = msg as NetNewBoard;
        GameController.size_row = nb.wid;
        GameController.size_col = nb.len;
        GameController.type_hero_black = nb.hero1Ability;
        GameController.type_hero_white = nb.hero2Ability;
        GameController.grid = new List<ChessPiece>();
        for (int i = 0; i < nb.wid; i++)
        {
            for (int j = 0; j < nb.len; j++)
            {
                if (nb.board[i, j] != ChessPieceType.None)
                {
                    ChessPiece tempPiece = new ChessPiece();
                    tempPiece.currentX = i;
                    tempPiece.currentY = j;
                    tempPiece.type = nb.board[i, j];
                    tempPiece.team = (j > nb.len / 2) ? 1 : 0;

                    Debug.Log($"Adding {tempPiece} at {i} : {j}");
                    GameController.grid.Add(tempPiece);
                }
            }
        }

        NotifCenter.notif.Enqueue("Player found");
        SceneManager.LoadScene(3);
    }
    private void OnChatClient(NetMessage msg)
    {
        var nc = msg as NetChat;
        ChatLog.inComeText = nc.msg;
    }
    #endregion
}
