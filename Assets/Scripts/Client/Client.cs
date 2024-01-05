using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    public enum StateType
    { TITLE, MENU, MATCHMAKING, LOBBY, INGAME };

    public static Client Instance { get; private set; }
    public NetworkingClient NetworkingClient => networkingClient;
    public CameraController CameraController => cameraController;
    public Fire Fire => fire;
    public Book Book => book;

    public void Init()
    {
        states[StateType.TITLE] = new ClientStateTitle(this);
        states[StateType.MENU] = new ClientStateTitle(this);
        states[StateType.MATCHMAKING] = new ClientStateTitle(this);
        states[StateType.LOBBY] = new ClientStateTitle(this);
        states[StateType.INGAME] = new ClientStateTitle(this);

        networkingClient.Init();
        SetAppState(StateType.TITLE);
    }

    public void Close()
    {
        DestroyImmediate(clientRoot.gameObject);
    }

    public void SetAppState(StateType stateType)
    {
        currentState?.Unset();
        currentState = states[stateType];
        currentState.Set();
    }

    [Header("References")]
    [SerializeField] private Transform clientRoot;
    [SerializeField] private NetworkingClient networkingClient;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Fire fire;
    [SerializeField] private Book book;

    private Dictionary<StateType, ClientState> states = new();
    private ClientState currentState;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void Update()
    {
        currentState?.Update();
    }
}
