using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkingClient : MonoBehaviour
{
    public Action OnConnect = () => { };
    public Action OnDisconnect = () => { };

    public enum ConnectionResponseType
    { SUCCESS, FAILED_CONNECT, FAILED_START }

    public bool IsStarted { get; private set; } = false;
    public bool IsConnected { get; private set; } = false;

    public void Init()
    {
        Config config = ConfigReader.ReadConfig();
        networkTransport.SetConnectionData(config.Address, config.Port);
        networkTransport.OnTransportEvent += OnTransportEvent;
    }

    public Task<ConnectionResponseType> StartConnection()
    {
        if (IsStarted || IsConnected) throw new Exception("Already IsStarted or IsConnected cannot TryConnect().");

        var connectionTCS = new TaskCompletionSource<ConnectionResponseType>();
        connectionResponse = (ConnectionResponseType res) =>
        {
            connectionResponse = null;
            connectionTCS.SetResult(res);
        };

        IsStarted = networkManager.StartClient();

        if (!IsStarted) connectionResponse(ConnectionResponseType.FAILED_START);

        return connectionTCS.Task;
    }

    public void Disconnect()
    {
        if (IsStarted || IsConnected) throw new Exception("Not IsStarted nor IsConnected so cannot Disconnect().");

        networkManager.Shutdown();
        IsStarted = false;
        IsConnected = false;
    }

    [Header("References")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private UnityTransport networkTransport;

    private Action<ConnectionResponseType> connectionResponse;

    private void OnTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
    {
        if (eventType == NetworkEvent.Connect)
        {
            connectionResponse?.Invoke(ConnectionResponseType.SUCCESS);
            OnConnect();
        }
        else if (eventType == NetworkEvent.Disconnect)
        {
            connectionResponse?.Invoke(ConnectionResponseType.FAILED_CONNECT);
            OnDisconnect();
        }
    }
}
