using System;
using UnityEngine;

[Serializable]
public class ClientStateMatchmaking : ClientState
{
    public override async void Set()
    {
        Debug.Log("Calling to Connect.");
        var task = client.NetworkingClient.StartConnection();
        Debug.Log("Connection Response: " + await task);
    }
}
