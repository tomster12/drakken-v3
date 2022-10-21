
using System;
using UnityEngine;
using Unity.Netcode;


[Serializable]
public class Match : NetworkBehaviour
{
    public static Match instance { get; private set; }
    public static Action OnSpawn;

    [Header("References")]
    [SerializeField] private NetworkObject net;

    private ulong clientId1, clientId2;


    public override void OnNetworkSpawn()
    {
        // Singleton handle
        instance = this;
        if (OnSpawn != null) OnSpawn();
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
        if (!IsServer) return false;

        // Check if Match contains a client
        return clientId1 == clientId || clientId2 == clientId;
    }


    public void Close_Serverside()
    {
        if (!IsServer) return;

        // Close match
        CloseMatch_ClientRpc();
        net.Despawn();
    }

    #endregion


    #region Client

    [ClientRpc]
    public void CloseMatch_ClientRpc()
    {
        Debug.Log("Closing down match clientside");

        // Remove this match from existence
        instance = null;
        OnSpawn = null;
        Matchmaker.instance.CallCloseMatch();
    }


    public void ReadyUp(Action callback)
    {
        // Pretend immediately ready
        callback();
    }

    #endregion
}
