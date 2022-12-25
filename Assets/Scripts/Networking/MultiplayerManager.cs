
using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;


public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager instance { get; private set; }
    public static Action<bool> OnTryConnect;
    public static Action OnDisconnect;

    [Header("References")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Transform clientParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject matchmakerPrefab;

    public bool isServer { get; private set; }


    private void Awake()
    {
        // Singleton handling
        if (instance != null) return;
        instance = this;
    }


    public void Init()
    {
        // Main initialization
        InitConfig();
        if (isServer) InitServer_Serverside();
        else InitClient();
    }

    private void InitConfig()
    {
        String path = Application.dataPath + "\\config.cfg";
        System.IO.StreamReader reader = new System.IO.StreamReader(path);
        UNetTransport unet = networkManager.GetComponent<UNetTransport>();
        String address = reader.ReadLine();
        String port = reader.ReadLine();
        isServer = reader.ReadLine() == "1";
        #if UNITY_EDITOR
        isServer |= ParrelSync.ClonesManager.GetArgument() == "server";
        #endif
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
        Matchmaker.instance.OnClientDisconnect_Serverside(clientId);
    }

    #endregion


    #region Client

    public bool isConnecting { get; private set; } = false;
    public bool isConnected { get; private set; } = false;
    public bool isSearching => Matchmaker.instance.isSearching;
    public bool hasMatch => Matchmaker.instance.hasMatch;


    private void InitClient()
    {
        // Add callbacks once for connection checking
        Matchmaker.OnSpawn += CheckConnected;
    }


    public bool TryConnect()
    {
        if (isConnected) return false;

        // Try connect to the server and wait for response
        isConnecting = true;
        if (!networkManager.StartClient())
        {
            isConnecting = false;
            if (OnTryConnect != null) OnTryConnect(false);
        }
        CheckConnected();
        return true;
    }

    private void CheckConnected()
    {
        if (!isConnecting) return;
        if (Matchmaker.instance == null) return;

        // All objects have spawned so have connected
        isConnecting = false;
        isConnected = true;
        if (OnTryConnect != null) OnTryConnect(true);
    }

    public bool Disconnect()
    {
        if (!isConnecting && !isConnected) return false;

        // Disconnect from server and immediately callback
        networkManager.Shutdown();
        isConnecting = false;
        isConnected = false;
        if (OnTryConnect != null) OnTryConnect(false);
        if (OnDisconnect != null) OnDisconnect();
        return true;
    }


    public bool TryFindMatch()
    {
        if (Matchmaker.instance == null) return false;
        if (!isConnected || isSearching) return false;
        if (hasMatch) return false;

        // Passthrough to the Matchmaker instance
        Matchmaker.instance.TryFindMatch();
        return true;
    }

    public bool TryLeaveMatchmaking()
    {
        if (!isConnected) return false;
        if (!isSearching && !hasMatch) return false;

        // Passthrough to the Matchmaker instance
        Matchmaker.instance.TryLeaveMatchmaking();
        return true;
    }

    #endregion
}
