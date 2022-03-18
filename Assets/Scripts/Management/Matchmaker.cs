
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Matchmaker : NetworkBehaviour
{
    // Declare variables
    public static Matchmaker instance;
    public static Action instanceInitialized;

    [Header("Prefabs")]
    [SerializeField] GameObject matchPrefab;


    public override void OnNetworkSpawn()
    {
        // Singleton handle
        instance = this;
        if (instanceInitialized != null) instanceInitialized();
    }


    #region Server

    // Declare variables
    private List<Match> matches = new List<Match>();
    private ulong waitingClientId0 = 0;
    private ulong waitingClientId1 = 0;
    private uint clientsWaiting = 0;


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
            ClientRpcParams clientRpcParams = new ClientRpcParams
                { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { waitingClientId0 } } };
            CheckMatchmaking_ClientRpc(clientRpcParams);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void CreateMatch_ServerRpc(ulong clientId)
    {
        if (clientsWaiting != 2 && clientId == waitingClientId0) return;

        // Create match object and initialize
        Debug.Log("Generating match for " + waitingClientId0 + " against " + waitingClientId1 + ".");
        GameObject matchGO = Instantiate(matchPrefab, Vector3.zero, Quaternion.identity);
        NetworkObject matchNO = matchGO.GetComponent<NetworkObject>();
        Match match = matchGO.GetComponent<Match>();
        match.SetClientIds_Serverside(waitingClientId0, waitingClientId1);
        matchNO.Spawn();
        matchNO.CheckObjectVisibility = (clientId) => {
            return clientId == waitingClientId0 || clientId == waitingClientId1;
        };
        matches.Add(match);

        // Update matchmaking variables
        waitingClientId0 = 0;
        waitingClientId1 = 0;
        clientsWaiting = 0;
    }


    public void ClientDisconnect_Serverside(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log("Left Matchmaking: " + clientId);

        // Deal with disconnection in progress
        if (clientId == waitingClientId0 || clientId == waitingClientId1)
        {
            // client stopped match making before any connection
            if (clientsWaiting == 1)
            {
                waitingClientId0 = 0;
                waitingClientId1 = 0;
                clientsWaiting = 0;
            }

            // Client stopped match making during connection so tell other client
            else if (clientsWaiting == 2)
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                        { TargetClientIds = new ulong[] { clientId == waitingClientId0 ? waitingClientId1 : waitingClientId0 } }
                };
                Debug.Log("Disconnecting " + clientRpcParams.Send.TargetClientIds[0]);
                StopMatchmaking_ClientRpc(clientRpcParams);
            }
        }

        // Delete any matches associated with id
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].ContainsClient_Serverside(clientId))
            {
                Match match = matches[i];
                matches.RemoveAt(i);
                match.Close_Serverside();
                break;
            }
        }
    }

    #endregion


    #region Client

    public void FindMatch() => FindMatch_ServerRpc();


    [ClientRpc]
    public void CheckMatchmaking_ClientRpc(ClientRpcParams clientRpcParams = default) => CreateMatch_ServerRpc(NetworkManager.Singleton.LocalClientId);


    [ClientRpc]
    public void StopMatchmaking_ClientRpc(ClientRpcParams clientRpcParams = default) => AppManager.instance.Disconnect();

    #endregion
}
