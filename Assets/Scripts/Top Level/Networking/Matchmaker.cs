
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Matchmaker : NetworkBehaviour
{
    public static Matchmaker instance;
    public static Action OnSpawn;
    public static Action OnLeaveMatch;

    [Header("References")]
    [SerializeField] MultiplayerManager multiplayerManager;

    [Header("Prefabs")]
    [SerializeField] GameObject matchPrefab;


    public override void OnNetworkSpawn()
    {
        // Singleton handle
        instance = this;
        if (OnSpawn != null) OnSpawn();
    }


    #region Server

    private List<Match> matches = new List<Match>();
    private ulong waitingClientId0 = 0;
    private ulong waitingClientId1 = 0;
    private uint clientsWaiting = 0;

    public void OnClientDisconnect_Serverside(ulong clientId) => LeaveMatchmaking_Serverside(clientId);


    [ServerRpc(RequireOwnership = false)]
    public void FindMatch_ServerRpc(ServerRpcParams serverRpcParams = default) 
    {
        ulong connectingClientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log("Finding match for client: " + connectingClientId);

        // Currently no players so waiting
        if (clientsWaiting == 0)
        {
            Debug.Log("Waiting for another client.");
            waitingClientId0 = connectingClientId;
            clientsWaiting = 1;
        }

        // Player waiting so generate match
        else if (clientsWaiting == 1)
        {
            Debug.Log("Checking other client connected.");
            waitingClientId1 = connectingClientId;
            clientsWaiting = 2;

            // Check waiting clients are still connected
            CreateMatch_Serverside();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LeaveMatchaking_ServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Call the serverside function
        ulong connectingClientId = serverRpcParams.Receive.SenderClientId;
        LeaveMatchmaking_Serverside(connectingClientId);
    }
    

    public void CreateMatch_Serverside()
    {
        if (waitingClientId0 == 0 || waitingClientId1 == 0 || clientsWaiting == 0) return;

        // Create match object and initialize
        Debug.Log("MMS: Generating match for " + waitingClientId0 + " against " + waitingClientId1 + ".");
        GameObject matchGO = Instantiate(matchPrefab, Vector3.zero, Quaternion.identity);
        NetworkObject matchNO = matchGO.GetComponent<NetworkObject>();
        Match match = matchGO.GetComponent<Match>();
        match.SetClientIds_Serverside(waitingClientId0, waitingClientId1);
        matchNO.CheckObjectVisibility = (clientId) =>
        {
            return clientId == waitingClientId0 || clientId == waitingClientId1;
        };
        matchNO.Spawn();
        matches.Add(match);

        // Update matchmaking variables
        waitingClientId0 = 0;
        waitingClientId1 = 0;
        clientsWaiting = 0;
    }

    public void LeaveMatchmaking_Serverside(ulong clientId)
    {
        if (!IsServer) return;
        Debug.Log("MMS: Left Matchmaking: " + clientId);

        // Leave match
        LeaveQueue_Serverside(clientId);
        LeaveMatch_Serverside(clientId);
    }

    public void LeaveQueue_Serverside(ulong clientId)
    {
        if (!IsServer) return;

        // Remove other client from queue
        if (clientId == waitingClientId0 || clientId == waitingClientId1)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { waitingClientId0, waitingClientId1 }
                }
            };
            Debug.Log("MMS: Stopping search for  " + clientRpcParams.Send.TargetClientIds[0]);
            OnStopSearch_ClientRpc(clientRpcParams);

            // Reset queue variables after sorting out other client
            waitingClientId0 = 0;
            waitingClientId1 = 0;
            clientsWaiting = 0;
        }
    }

    public void LeaveMatch_Serverside(ulong clientId)
    {
        if (!IsServer) return;

        // Delete any matches associated with id
        Debug.Log("MMS: Removing from match: " + clientId);
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
    private Action<bool> matchSearchCallback;


    public void FindMatch(Action<bool> callback)
    {
        if (hasMatch || isSearching) { callback(true); return; }

        // Start searching and cache callback
        Debug.Log("MM: Starting match search");
        isSearching = true;
        matchSearchCallback = callback;
        Match.OnSpawn += OnFoundMatch;
        FindMatch_ServerRpc();
    }

    public void LeaveMatchmaking()
    {
        if (!hasMatch && !isSearching) return;

        Debug.Log("MM: Leaving match search");
        LeaveMatchaking_ServerRpc();
    }


    [ClientRpc]
    public void OnStopSearch_ClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!isSearching || hasMatch) return;

        // Stop searching and call callback
        Debug.Log("MM: Stopping match search midway");
        isSearching = false;
        Match.OnSpawn -= OnFoundMatch;
        if (matchSearchCallback != null) matchSearchCallback(false);
    }

    public void OnFoundMatch()
    {
        if (!isSearching || hasMatch) return;

        // Found match so stop searching
        Debug.Log("MM: Found a match");
        hasMatch = true;
        isSearching = false;
        Match.OnSpawn -= OnFoundMatch;
        if (matchSearchCallback != null) matchSearchCallback(true);
    }

    public void OnCloseMatch()
    {
        if (isSearching || !hasMatch) return;

        // Match currently in has closed
        Debug.Log("MM: Match has been closed");
        hasMatch = false;
        if (OnLeaveMatch != null) OnLeaveMatch();
    }

    #endregion
}
