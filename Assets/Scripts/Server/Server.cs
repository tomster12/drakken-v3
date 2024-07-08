using UnityEngine;

public class Server : MonoBehaviour
{
    public NetworkingServer NetworkingServer => networkingServer;

    public void Init()
    {
        networkingServer.Init();
        networkingServer.OnClientConnected += OnClientConnected;
        networkingServer.OnClientDisconnected += OnClientDisconnected;
    }

    public void Close()
    {
        DestroyImmediate(serverRoot.gameObject);
    }

    [Header("References")]
    [SerializeField] private Transform serverRoot;
    [SerializeField] private NetworkingServer networkingServer;
    [SerializeField] private GameObject matchmakerPrefab;

    private void Start()
    {
        networkingServer.StartListening();

        //GameObject matchmakerGO = Instantiate(matchmakerPrefab, Vector3.zero, Quaternion.identity);
        //matchmakerGO.GetComponent<NetworkObject>().Spawn();
    }

    private void OnClientConnected(ulong clientID)
    {
        Debug.Log("Client Connected.");
    }

    private void OnClientDisconnected(ulong clientID)
    {
        Debug.Log("Client Disconnected.");
    }
}
