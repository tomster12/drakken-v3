using UnityEngine;

public class Server : MonoBehaviour
{
    public static Server Instance { get; private set; }
    public NetworkingServer NetworkingServer => networkingServer;

    public void Init()
    {
        networkingServer.Init();
        networkingServer.StartListening();
    }

    public void Close()
    {
        DestroyImmediate(serverRoot.gameObject);
    }

    [Header("References")]
    [SerializeField] private Transform serverRoot;
    [SerializeField] private NetworkingServer networkingServer;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }
}
