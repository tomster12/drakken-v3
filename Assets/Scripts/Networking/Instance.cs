using Unity.Netcode;
using UnityEngine;

public class Instance : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner) RandomMovement_ServerRpc();
    }

    private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();

    private void Update()
    {
        if (IsOwner && Input.GetMouseButtonDown(1)) RandomMovement_ServerRpc();
        transform.position = position.Value;
    }

    [ServerRpc]
    private void RandomMovement_ServerRpc()
    {
        position.Value = (new Vector3(Random.Range(-5f, 5f), 10f, 6f));
    }
}
