using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class Match : NetworkBehaviour
{
    public static Action OnSpawn;
    public static Match instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        // Singleton handle
        instance = this;
        if (OnSpawn != null) OnSpawn();
    }

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

    [ClientRpc]
    public void CloseMatch_ClientRpc()
    {
        Debug.Log("Closing down match clientside");

        // Remove this match from existence
        instance = null;
        OnSpawn = null;
        //Matchmaker.Instance.CallCloseMatch();
    }

    public void ReadyUp(Action callback)
    {
        // Pretend immediately ready
        callback();
    }

    [Header("References")]
    [SerializeField] private NetworkObject net;

    private ulong clientId1, clientId2;
}
