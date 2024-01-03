using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public static Action<bool> OnTryConnect;
    public static Action OnDisconnect;
    public static MultiplayerManager instance { get; private set; }
    public bool IsServer { get; private set; }
    public bool IsConnecting { get; private set; } = false;
    public bool IsConnected { get; private set; } = false;
    public bool IsSearching => Matchmaker.Instance.IsSearching;
    public bool HasMatch => Matchmaker.Instance.HasMatch;

    public void Init()
    {
        // Main initialization
        InitConfig();
        if (IsServer) InitServer_Serverside();
        else InitClient();
    }

    public bool TryConnect()
    {
        if (IsConnected) return false;

        // Try connect to the server and wait for response
        IsConnecting = true;
        if (!networkManager.StartClient())
        {
            IsConnecting = false;
            if (OnTryConnect != null) OnTryConnect(false);
        }
        CheckConnected();
        return true;
    }

    public bool Disconnect()
    {
        if (!IsConnecting && !IsConnected) return false;

        // Disconnect from server and immediately callback
        networkManager.Shutdown();
        IsConnecting = false;
        IsConnected = false;
        if (OnTryConnect != null) OnTryConnect(false);
        if (OnDisconnect != null) OnDisconnect();
        return true;
    }

    public bool TryFindMatch()
    {
        if (Matchmaker.Instance == null) return false;
        if (!IsConnected || IsSearching) return false;
        if (HasMatch) return false;

        // Passthrough to the Matchmaker instance
        Matchmaker.Instance.TryFindMatch();
        return true;
    }

    public bool TryLeaveMatchmaking()
    {
        if (!IsConnected) return false;
        if (!IsSearching && !HasMatch) return false;

        // Passthrough to the Matchmaker instance
        Matchmaker.Instance.TryLeaveMatchmaking();
        return true;
    }

    [Header("References")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Transform clientParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject matchmakerPrefab;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    private void InitConfig()
    {
        String path = Application.dataPath + "\\config.cfg";
        System.IO.StreamReader reader = new System.IO.StreamReader(path);
        UnityTransport unet = networkManager.GetComponent<UnityTransport>();
        String address = reader.ReadLine();
        String port = reader.ReadLine();
        IsServer = reader.ReadLine() == "1";
        // isServer |= ParrelSync.ClonesManager.GetArgument() == "server";
        // unet.ConnectAddress = address;
        // unet.ConnectPort = int.Parse(port);
        // unet.ServerListenPort = int.Parse(port);
        reader.Close();
    }

    #region Server

    private void InitServer_Serverside()
    {
        // Add event listeners
        networkManager.OnClientConnectedCallback += OnClientConnected_Serverside;
        networkManager.OnClientDisconnectCallback += OnClientDisconnect_Serverside;

        // Start connection as server
        Debug.Log("Hosting...");
        networkManager.StartServer();
        clientParent.gameObject.SetActive(false);

        // Initialize matchmaker
        GameObject matchmakerGO = Instantiate(matchmakerPrefab, Vector3.zero, Quaternion.identity);
        matchmakerGO.GetComponent<NetworkObject>().Spawn();
    }

    private void OnClientConnected_Serverside(ulong clientId)
    {
        if (!networkManager.IsServer) return;

        // Client has connected to the server
        Debug.Log("Client Connected: " + clientId);
    }

    private void OnClientDisconnect_Serverside(ulong clientId)
    {
        if (!networkManager.IsServer) return;

        // Stop matchmaking and close match
        Debug.Log("Client Disconnected: " + clientId);
        Matchmaker.Instance.OnClientDisconnect_Serverside(clientId);
    }

    #endregion Server

    #region Client

    private void InitClient()
    {
        // Add callbacks once for connection checking
        Matchmaker.OnSpawn += CheckConnected;
    }

    private void CheckConnected()
    {
        if (!IsConnecting) return;
        if (Matchmaker.Instance == null) return;

        // All objects have spawned so have connected
        IsConnecting = false;
        IsConnected = true;
        if (OnTryConnect != null) OnTryConnect(true);
    }

    #endregion Client
}
