
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Networking.Transport;
using System;

public class Client : MonoBehaviour
{
    #region Singleton implementation
    public static Client Instance { set; get; }

    private void Awake()
    {
        Instance = this;
    }
    #endregion



    public NetworkDriver driver;
    private NetworkConnection connection;

    private bool isActive = false;

    public Action connectionDropped;

    //methods
    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);

        connection = driver.Connect(endpoint);

        Debug.Log("Attempting to connect to server on " + endpoint.Address);

        isActive = true;

        RegisterToEvent();
    }
    public void Shutdown()
    {
        if (isActive)
        {
            Debug.Log("[CLIENT] Shutting down Client");
            UnregisterToEvent();
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection);
        }
    }
    public void OnDestroy()
    {
        Debug.Log("[CLIENT] OnDestroy called");
        Shutdown();
    }

    public void Update()
    {
        if (!isActive)
            return;

        CheckAlive();

        driver.ScheduleUpdate().Complete();

        UpdateMessagePump();
    }
    private void CheckAlive()
    {
        if (!connection.IsCreated && isActive)
        {
            Debug.Log("Lost connection to server");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }
    private void UpdateMessagePump()
    {
        DataStreamReader stream;

        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                SendToServer(new NetWelcome());
                Debug.Log("[CLIENT] We're connected");

                NetID ni = new NetID();
                ni.PlayerID = 10;
                SendToServer(ni);
                Debug.Log("[CLIENT] Sent ID to server");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                NetUtility.OnData(stream, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                OnDisconnect();
            }
        }
    }
    public void SendToServer(NetMessage msg)
    {
        Debug.Log($"[CLIENT] Sending {msg} to SERVER");
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    // event parsing
    private void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }
    private void UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }
    private void OnKeepAlive(NetMessage nm)
    {
        SendToServer(nm);
    }
    public void OnDisconnect()
    {
        Debug.Log("Client got disconnected from server");
        connection = default(NetworkConnection);
        connectionDropped?.Invoke();
        Shutdown();
    }
}
