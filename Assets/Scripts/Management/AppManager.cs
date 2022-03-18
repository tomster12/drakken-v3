
using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;


public class AppManager : MonoBehaviour
{
    // Declare variables
    public static AppManager instance;

    [Header("References")]
    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameManager gameManager;
    [SerializeField] Transform clientParent;
    [SerializeField] Book book;

    [Header("Prefabs")]
    [SerializeField] GameObject matchmakerPrefab;

    [Header("Config")]
    [SerializeField] private bool hostServer;



    #region - Main

    private void Start()
    {
        // Server initialization
        instance = this;
        ReadConfig();
        if (hostServer) SetupServer();
        else SetupClient();

        // Add event listeners
        networkManager.OnClientConnectedCallback += OnClientConnected_Serverside;
        networkManager.OnClientDisconnectCallback += OnClientDisconnect_Serverside;
    }


    private void ReadConfig()
    {
        System.IO.StreamReader reader = new System.IO.StreamReader(Application.dataPath + "\\config.cfg");
        UNetTransport unet = networkManager.GetComponent<UNetTransport>();
        String address = reader.ReadLine();
        String port = reader.ReadLine();
        unet.ConnectAddress = address;
        unet.ConnectPort = int.Parse(port);
        unet.ServerListenPort = int.Parse(port);
        reader.Close();
    }

    #endregion


    #region Server

    private void SetupServer()
    {
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
        Matchmaker.instance.ClientDisconnect_Serverside(clientId);
    }

    #endregion


    #region Client
    
    // Declare variables
    private bool isConnecting;
    private Action connectionCallback0;
    private Action connectionCallback1;


    private void SetupClient()
    {
        // Client initialization
        book.SetBookState(new Book.BookStateUnopened(book, true));
    }


    public void TryConnect(Action<int> statusCallback)
    {
        // Connect as a client
        Debug.Log("Connecting...");
        bool clientStarted = networkManager.StartClient();

        // Could not connect to server
        if (!clientStarted)
        {
            Debug.Log("Could not connect.");
            statusCallback(0);
            return;
        }

        // Successfully connected so wait for network objects
        isConnecting = true;

        // Matchmake when detected
        connectionCallback0 = () =>
        {
            Matchmaker.instanceInitialized -= connectionCallback0;
            connectionCallback0 = null;

            // Find a match from the server
            Debug.Log("Finding a match...");
            statusCallback(1);
            Matchmaker.instance.FindMatch();
        };
        Matchmaker.instanceInitialized += connectionCallback0;

        // Cache match and start when detected
        connectionCallback1 = () =>
        {
            Match.instanceInitialized -= connectionCallback1;
            connectionCallback1 = null;
            
            // Update gameManager and start
            Debug.Log("Match found!");
            gameManager.SetMatch(Match.instance);
            statusCallback(2);
            isConnecting = false;
        };
        Match.instanceInitialized += connectionCallback1;
    }


    public void Disconnect()
    {
        Debug.Log("Disconnecting...");

        // Remove callbacks on Matchmaker / Match
        if (isConnecting)
        {
            Matchmaker.instanceInitialized -= connectionCallback0;
            connectionCallback0 = null;
            Match.instanceInitialized -= connectionCallback1;
            connectionCallback1 = null;
            isConnecting = false;
        }

        // Update other scripts about disconnection
        networkManager.Shutdown();
        book.OnDisconnect();
    }


    public bool StartGame(ClassData currentClass_, Book.BookStateIngame ingameBook_)
    {
        // Start game
        gameManager.StartGame(currentClass_, ingameBook_);
        return true;
    }

    #endregion
}
