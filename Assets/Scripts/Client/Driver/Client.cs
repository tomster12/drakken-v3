using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    public enum StateType
    { TITLE, MENU, MATCHMAKING, LOBBY, INGAME };

    public NetworkingClient NetworkingClient => networkingClient;

    public void Init()
    {
        states = new Dictionary<StateType, ClientState>();
        states[StateType.TITLE] = stateTitle;
        states[StateType.MENU] = stateMenu;
        states[StateType.MATCHMAKING] = stateMatchmaking;
        states[StateType.LOBBY] = stateLobby;
        states[StateType.INGAME] = stateIngame;
        foreach (var state in states) state.Value.Init(this);

        networkingClient.Init();
    }

    public void Close()
    {
        DestroyImmediate(clientRoot.gameObject);
    }

    public void SetState(StateType stateType)
    {
        currentState?.Unset();
        currentState = states[stateType];
        currentState.Set();
    }

    [Header("States")]
    [SerializeField] private ClientStateTitle stateTitle;
    [SerializeField] private ClientStateMenu stateMenu;
    [SerializeField] private ClientStateMatchmaking stateMatchmaking;
    [SerializeField] private ClientStateLobby stateLobby;
    [SerializeField] private ClientStateIngame stateIngame;

    [Header("References")]
    [SerializeField] private Transform clientRoot;
    [SerializeField] private NetworkingClient networkingClient;

    private Dictionary<StateType, ClientState> states;
    private ClientState currentState;

    private void Start()
    {
        SetState(StateType.TITLE);
    }

    private void Update() => currentState?.Update();
}
