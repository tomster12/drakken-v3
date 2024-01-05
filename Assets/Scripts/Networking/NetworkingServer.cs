using NUnit.Framework;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkingServer : MonoBehaviour
{
    public static NetworkingServer Instance { get; private set; }

    public bool IsConfigLoaded { get; private set; }
    public bool IsStarted { get; private set; }
    public bool IsListening { get; private set; }

    public void Init()
    {
        LoadConfig();
    }

    public void StartListening()
    {
        if (IsListening) throw new Exception("Already IsListening cannot StartListening().");

        Debug.Log("Listening...");

        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        networkManager.StartServer();

        GameObject matchmakerGO = Instantiate(matchmakerPrefab, Vector3.zero, Quaternion.identity);
        matchmakerGO.GetComponent<NetworkObject>().Spawn();

        IsListening = true;
    }

    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private GameObject matchmakerPrefab;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void LoadConfig()
    {
        if (IsConfigLoaded) throw new Exception("Config already loaded.");
        String path = Application.dataPath + "\\config.cfg";
        System.IO.StreamReader reader = new System.IO.StreamReader(path);
        UnityTransport unet = networkManager.GetComponent<UnityTransport>();
        String address = reader.ReadLine();
        String port = reader.ReadLine();
        Assert.True(reader.ReadLine() == "1");
        // isServer |= ParrelSync.ClonesManager.GetArgument() == "server";
        // unet.ConnectAddress = address;
        // unet.ConnectPort = int.Parse(port);
        // unet.ServerListenPort = int.Parse(port);
        reader.Close();
        IsConfigLoaded = true;
    }

    private void OnClientConnected(ulong clientId)
    {
        // Client has connected to the server
        Debug.Log("Client Connected: " + clientId);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        // Stop matchmaking and close match
        Debug.Log("Client Disconnected: " + clientId);
        Matchmaker.Instance.OnClientDisconnect_Serverside(clientId);
    }
}
