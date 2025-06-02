using System.Net;
using UnityEngine;
using Unity.Netcode.Community.Discovery;

public class MyNetworkDiscovery : NetworkDiscovery<DiscoveryBroadcastData, DiscoveryResponseData>
{
    public string serverName = "LAN Game";
    public System.Action<IPEndPoint, DiscoveryResponseData> OnServerFound;

    protected override bool ProcessBroadcast(IPEndPoint sender, DiscoveryBroadcastData broadCast, out DiscoveryResponseData response)
    {
        response = new DiscoveryResponseData
        {
            ServerName = serverName,
            ServerAddress = sender.Address.ToString()
        };
        return true; // Always respond
    }

    protected override void ResponseReceived(IPEndPoint sender, DiscoveryResponseData response)
    {
        OnServerFound?.Invoke(sender, response);
    }
    
}