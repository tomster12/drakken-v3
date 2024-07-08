using System;
using UnityEngine;
using Unity.Multiplayer.Playmode;
using System.Linq;

internal class ConfigReader
{
    private static Config config;

    public static Config ReadConfig()
    {
        String path = Application.dataPath + "\\config.cfg";
        var reader = new System.IO.StreamReader(path);
        bool isServer = reader.ReadLine() == "1";
        String address = reader.ReadLine();
        ushort port = ushort.Parse(reader.ReadLine());
        String listenAddress = reader.ReadLine();
        reader.Close();

        if (CurrentPlayer.ReadOnlyTags().Contains("Server")) isServer = true;

        config = new Config(isServer, address, port, listenAddress);
        return config;
    }
}

internal struct Config
{
    public bool IsServer { get; private set; }
    public String Address { get; private set; }
    public ushort Port { get; private set; }
    public String ListenAddress { get; private set; }

    public Config(bool isServer, String address, ushort port, String listenAddress)
    {
        IsServer = isServer;
        Address = address;
        Port = port;
        ListenAddress = listenAddress;
    }
};
