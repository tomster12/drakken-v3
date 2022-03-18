
using System;
using UnityEngine;
using Unity.Netcode;


[Serializable]
public class Match : NetworkBehaviour
{
    // Declare variables
    public static Match instance;
    public static Action instanceInitialized;

    [SerializeField] private ulong clientId1, clientId2; // Temporary serialize to show


    public override void OnNetworkSpawn()
    {
        // Singleton handle
        instance = this;
        if (instanceInitialized != null) instanceInitialized();
    }


    #region Server

    public void SetClientIds_Serverside(ulong clientId1_, ulong clientId2_)
    {
        if (!IsServer) return;

        // Initialize client ids
        clientId1 = clientId1_;
        clientId2 = clientId2_;
    }

    public bool ContainsClient_Serverside(ulong clientId)
    {
        // Check if Match contains a client
        return clientId1 == clientId || clientId2 == clientId;
    }


    public void Close_Serverside()
    {
        if (!IsServer) return;

        // Close match
        Close_ClientRpc();
    }

    #endregion


    #region Client

    public void ReadyUp(Action callback)
    {
        // Pretend immediately ready
        callback();
    }


    [ClientRpc]
    public void Close_ClientRpc()
    {
        // Match forcibly closed
        Debug.Log("Match forcibly closed.");
        AppManager.instance.Disconnect();
    }

    #endregion
}
