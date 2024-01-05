using NUnit.Framework;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkingClient : MonoBehaviour
{
    public static NetworkingClient Instance { get; private set; }
    public static Action<bool> OnConnectResponse = (bool isConnected) => { };
    public static Action OnDisconnect = () => { };

    public bool IsConfigLoaded { get; private set; }
    public bool IsConnecting { get; private set; } = false;
    public bool IsConnected { get; private set; } = false;

    public void Init()
    {
        LoadConfig();
    }

    public bool TryConnect()
    {
        if (!IsConfigLoaded) return false;
        if (IsConnected) return false;

        // Connection starting, so listen out for matchmaker
        IsConnecting = true;
        Matchmaker.OnSpawn += CheckConnectionEstablished;

        // Try start client, and fail early if cannot
        if (!networkManager.StartClient())
        {
            IsConnecting = false;
            OnConnectResponse(false);
            Matchmaker.OnSpawn -= CheckConnectionEstablished;
        }

        CheckConnectionEstablished();
        return true;
    }

    public bool Disconnect()
    {
        if (!IsConnecting && !IsConnected) return false;

        networkManager.Shutdown();
        if (IsConnecting) OnConnectResponse(false);
        IsConnecting = false;
        if (IsConnected) OnDisconnect();
        IsConnected = false;
        return true;
    }

    [Header("References")]
    [SerializeField] private NetworkManager networkManager;

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
        Assert.True(reader.ReadLine() != "1");
        // isServer |= ParrelSync.ClonesManager.GetArgument() == "server";
        // unet.ConnectAddress = address;
        // unet.ConnectPort = int.Parse(port);
        // unet.ServerListenPort = int.Parse(port);
        reader.Close();
        IsConfigLoaded = true;
    }

    private void CheckConnectionEstablished()
    {
        if (!IsConnecting) return;

        // Matchmaker can only exist when connected
        if (Matchmaker.Instance == null) return;

        IsConnecting = false;
        IsConnected = true;
        OnConnectResponse(true);
    }
}
