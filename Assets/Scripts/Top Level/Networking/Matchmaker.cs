
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class Matchmaker : NetworkBehaviour
{
    public static Matchmaker instance { get; private set; }
    public static Action OnSpawn;
    public static Action<bool> OnFindMatch;
    public static Action OnCloseMatch;

    [Header("Prefabs")]
    [SerializeField] private GameObject matchPrefab;


    public override void OnNetworkSpawn()
    {
        // Singleton handle
        instance = this;
        if (OnSpawn != null) OnSpawn();
    }

    public override void OnNetworkDespawn()
    {
        // Cleanup singleton
        instance = null;

        // Handle clientside cleanup on despawn
        if (!MultiplayerManager.instance.isServer)
        {
            Debug.Log("<- Matchmaker disconnected so cleanup");
            StopFindMatch();
            CallCloseMatch();
        }
    }


    #region Server

    private List<Match> matches = new List<Match>();
    private ulong waitingClientId0 = 0;
    private ulong waitingClientId1 = 0;
    private uint clientsWaiting = 0;


    public void OnClientDisconnect_Serverside(ulong clientId) => TryLeaveMatchmaking_Serverside(clientId);


    [ServerRpc(RequireOwnership = false)]
    public void TryFindMatch_ServerRpc(ServerRpcParams serverRpcParams = default) 
    {
        ulong connectingClientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log("Finding match for client: " + connectingClientId);

        // Currently no players so waiting
        if (clientsWaiting == 0)
        {
            waitingClientId0 = connectingClientId;
            clientsWaiting = 1;
        }

        // Player waiting so generate match
        else if (clientsWaiting == 1)
        {
            waitingClientId1 = connectingClientId;
            clientsWaiting = 2;

            // Check waiting clients are still connected
            CreateMatch_Serverside();
        }
    }
    
    private void CreateMatch_Serverside()
    {
        if (waitingClientId0 == 0 || waitingClientId1 == 0 || clientsWaiting == 0) return;

        // Create match object and initialize
        Debug.Log("Generating match for " + waitingClientId0 + " against " + waitingClientId1 + ".");
        GameObject matchGO = Instantiate(matchPrefab, Vector3.zero, Quaternion.identity);
        NetworkObject matchNO = matchGO.GetComponent<NetworkObject>();
        Match match = matchGO.GetComponent<Match>();
        matchNO.CheckObjectVisibility = (clientId) =>
        {
            return clientId == waitingClientId0 || clientId == waitingClientId1;
        };
        matchNO.Spawn();
        match.SetClientIds_Serverside(waitingClientId0, waitingClientId1);
        matches.Add(match);

        // Update matchmaking variables
        waitingClientId0 = 0;
        waitingClientId1 = 0;
        clientsWaiting = 0;
    }


    [ServerRpc(RequireOwnership = false)]
    public void TryLeaveMatchmaking_ServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Call the serverside function
        ulong connectingClientId = serverRpcParams.Receive.SenderClientId;
        TryLeaveMatchmaking_Serverside(connectingClientId);
    }
    
    public void TryLeaveMatchmaking_Serverside(ulong clientId)
    {
        if (!IsServer) return;

        // Leave match
        Debug.Log("Left Matchmaking: " + clientId);
        LeaveQueue_Serverside(clientId);
        LeaveMatch_Serverside(clientId);
    }

    private void LeaveQueue_Serverside(ulong clientId)
    {
        if (!IsServer) return;
        if (clientId != waitingClientId0 && clientId != waitingClientId1) return;

        // Tell the other client to stop queueing
        Debug.Log("Removing " + clientId + " from the queue");
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId == waitingClientId1 ? waitingClientId0 : waitingClientId1 }
            }
        };
        StopFindMatch_ClientRpc(clientRpcParams);

        // Reset queue variables after sorting out other client
        waitingClientId0 = 0;
        waitingClientId1 = 0;
        clientsWaiting = 0;
    }

    private void LeaveMatch_Serverside(ulong clientId)
    {
        if (!IsServer) return;

        // Close any matches associated with id
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].ContainsClient_Serverside(clientId))
            {
                matches[i].Close_Serverside();
                matches.RemoveAt(i);
                break;
            }
        }
    }

    #endregion


    #region Client

    public bool isSearching { get; private set; } = false;
    public bool hasMatch { get; private set; } = false;


    public bool TryFindMatch()
    {
        if (hasMatch || isSearching) return false;

        // Ask the server to find a match and wait for response
        Debug.Log("-> Finding a match...");
        isSearching = true;
        Match.OnSpawn += CallFoundMatch;
        TryFindMatch_ServerRpc();
        return true;
    }

    public bool TryLeaveMatchmaking()
    {
        if (!hasMatch && !isSearching) return false;

        // Ask the server to leave matchmaking and wait for response
        Debug.Log("-> Leaving match search...");
        TryLeaveMatchmaking_ServerRpc();
        return true;
    }


    [ClientRpc]
    public void StopFindMatch_ClientRpc(ClientRpcParams clientRpcParams = default) => StopFindMatch();

    public void StopFindMatch()
    {
        if (!isSearching || hasMatch) return;

        // Server told client it is no longer finding a match
        Debug.Log("<- Stopping match search midway");
        isSearching = false;
        Match.OnSpawn -= CallFoundMatch;
        if (OnFindMatch != null) OnFindMatch(false);
    }

    public void CallFoundMatch()
    {
        if (!isSearching || hasMatch) return;

        // Match told client that it now exists
        Debug.Log("<- Found a match");
        hasMatch = true;
        isSearching = false;
        Match.OnSpawn -= CallFoundMatch;
        if (OnFindMatch != null) OnFindMatch(true);
    }

    public void CallCloseMatch()
    {
        if (isSearching || !hasMatch) return;

        // Match told client that it has been closed
        Debug.Log("<- Match has been closed");
        hasMatch = false;
        if (OnCloseMatch != null) OnCloseMatch();
    }

    #endregion
}
