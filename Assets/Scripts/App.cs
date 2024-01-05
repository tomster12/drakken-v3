using UnityEngine;

public class App : MonoBehaviour
{
    [SerializeField] private Client client;
    [SerializeField] private Server server;

    private void Awake()
    {
        if (CheckIsClient())
        {
            server.Close();
            client.Init();
        }
        else
        {
            client.Close();
            server.Init();
        }
    }

    private bool CheckIsClient()
    {
        return true;
    }
}
