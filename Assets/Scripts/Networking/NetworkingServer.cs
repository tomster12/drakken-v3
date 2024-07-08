using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkingServer : MonoBehaviour
{
    public Action<ulong> OnClientConnected = delegate { };
    public Action<ulong> OnClientDisconnected = delegate { };

    public bool IsListening { get; private set; }

    public void Init()
    {
        Config config = ConfigReader.ReadConfig();
        networkTransport.SetConnectionData(config.Address, config.Port, config.ListenAddress);
        networkTransport.OnTransportEvent += OnTransportEvent;
        networkManager.OnClientConnectedCallback += (ulong clientID) => { OnClientConnected(clientID); };
        networkManager.OnClientDisconnectCallback += (ulong clientID) => { OnClientDisconnected(clientID); };
    }

    public void StartListening()
    {
        if (IsListening) throw new Exception("Already IsStarted or IsListening cannot StartListening().");

        IsListening = networkManager.StartServer();
        if (!IsListening) throw new Exception("Could not StartServer()");
    }

    [Header("References")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private UnityTransport networkTransport;

    private void OnTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
    {
        Debug.Log(eventType);
    }
}
