using System.Collections.Generic;
using UnityEngine;

public partial class Client : MonoBehaviour
{
    public enum StateType
    { TITLE, MENU, MATCHMAKING, LOBBY, INGAME };

    public static Client instance { get; private set; }
    public MultiplayerManager MultiplayerManager => multiplayerManager;
    public GameManager GameManager => gameManager;
    public CameraController CameraController => cameraController;
    public Fire Fire => fire;
    public Book Book => book;

    public void SetAppState(StateType stateType)
    {
        currentState?.Unset();
        currentState = states[stateType];
        currentState.Set();
    }

    [Header("References")]
    [SerializeField] private MultiplayerManager multiplayerManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Fire fire;
    [SerializeField] private Book book;

    private Dictionary<StateType, ClientState> states = new();
    private ClientState currentState;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;

        states[StateType.TITLE] = new ClientStateTitle(this);
        states[StateType.MENU] = new ClientStateTitle(this);
        states[StateType.MATCHMAKING] = new ClientStateTitle(this);
        states[StateType.LOBBY] = new ClientStateTitle(this);
        states[StateType.INGAME] = new ClientStateTitle(this);

        multiplayerManager.Init();
        if (multiplayerManager.IsServer) return;

        SetAppState(StateType.TITLE);
    }

    private void Update()
    {
        currentState?.Update();
    }
}
