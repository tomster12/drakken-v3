
using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;


public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager instance;
    public static Action OnDisconnect;

    [Header("References")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Transform clientParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject matchmakerPrefab;

    [Header("Config")]
    [SerializeField] private bool _isServer;
    public bool isServer => _isServer;


    public void Awake()
    {
        // Singleton handling
        if (instance != null) return;
        instance = this;
    }

    public void Init(string configFileName)
    {
        // Main initialization
        ReadConfig(configFileName);
        if (isServer) InitServer_Serverside();
        else InitClient();
    }


    private void ReadConfig(string configFilePath)
    {
        System.IO.StreamReader reader = new System.IO.StreamReader(configFilePath);
        UNetTransport unet = networkManager.GetComponent<UNetTransport>();
        String address = reader.ReadLine();
        String port = reader.ReadLine();
        _isServer = isServer || (reader.ReadLine() == "1");
        unet.ConnectAddress = address;
        unet.ConnectPort = int.Parse(port);
        unet.ServerListenPort = int.Parse(port);
        reader.Close();
    }


    #region Server

    private void InitServer_Serverside()
    {
        // Add event listeners
        networkManager.OnClientConnectedCallback += OnClientConnected_Serverside;
        networkManager.OnClientDisconnectCallback += OnClientDisconnect_Serverside;

        // Start connection as server
        Debug.Log("Ml: Hosting...");
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
        Debug.Log("Ml: Client Connected: " + clientId);
    }

    private void OnClientDisconnect_Serverside(ulong clientId)
    {
        if (!networkManager.IsServer) return;

        // Stop matchmaking and close match
        Debug.Log("Ml: Client Disconnected: " + clientId);
        Matchmaker.instance.OnClientDisconnect_Serverside(clientId);
    }

    #endregion


    #region Client

    public bool isConnected { get; private set; } = false;
    public bool isSearching => Matchmaker.instance.isSearching;
    public bool hasMatch => Matchmaker.instance.hasMatch;


    private void InitClient() { }


    public void TryConnect(Action<bool> callback)
    {
        if (isConnected) { callback(true); return; }

        // Connect to server
        Debug.Log("Ml: Connecting to server");
        bool connectionCreated = networkManager.StartClient();
        if (!connectionCreated) { callback(false); return; }

        // Wait for network objects
        Matchmaker.OnSpawn += delegate ()
        {
            // All network objects spawned
            Debug.Log("Ml: Connected!");
            isConnected = true;
            callback(true);
        };
    }

    public void Disconnect()
    {
        if (!isConnected) return;

        // Disconnect and call event
        Debug.Log("Ml: Disconnecting from server");
        LeaveMatchmaking();
        networkManager.Shutdown();
        isConnected = false;
        if (OnDisconnect != null) OnDisconnect();
    }


    public void FindMatch(Action<bool> callback)
    {
        if (Matchmaker.instance == null) { callback(false); return; }
        if (!isConnected || isSearching) { callback(false); return; }
        if (hasMatch) { callback(true); return; }

        // Try find a match on the matchmaker
        Debug.Log("Ml: Searching for a match");
        Matchmaker.instance.FindMatch(callback);
    }

    public void LeaveMatchmaking()
    {
        if (!isConnected) return;
        if (!isSearching && !hasMatch) return;

        // Tell matchmaker to leave the match
        Debug.Log("Ml: Leaving matchmaking");
        Matchmaker.instance.LeaveMatchmaking();
    }

    #endregion
}
