using Unity.Netcode;
using UnityEngine;

public class Instance : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Local update functions
        _position.OnValueChanged += UpdateLocalPosition;

        if (!IsOwner) return;

        // Random move on initialization
        RandomMovement_ServerRpc();
    }

    
    private NetworkVariable<Vector3> _position = new NetworkVariable<Vector3>();

    private void Update()
    {
        if (!IsOwner) return;

        // Move randomly on right click
        if (Input.GetMouseButtonDown(1)) RandomMovement_ServerRpc();
    }

    [ServerRpc]
    private void RandomMovement_ServerRpc()
    {
        // Set position to random in range
        _position.Value = (new Vector3(Random.Range(-5f, 5f), 10f, 6f));
    }

    private void UpdateLocalPosition(Vector3 oldPos, Vector3 newPos) => transform.position = newPos;
}
