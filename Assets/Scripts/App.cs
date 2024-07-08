using System;
using UnityEngine;

public class App : MonoBehaviour
{
    [SerializeField] private Client client;
    [SerializeField] private Server server;

    private void Awake()
    {
        Config config = ConfigReader.ReadConfig();
        if (config.IsServer)
        {
            client.Close();
            server.Init();
        }
        else
        {
            server.Close();
            client.Init();
        }
    }
}
