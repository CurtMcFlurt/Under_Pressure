using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.Netcode.Community.Discovery;

public class LANDiscoveryManager : MonoBehaviour
{
    public NetworkManager networkManager;
    public Button hostButton;
    public Button joinButton;
    public Button refreshButton;
    public Transform sessionListParent;
    public GameObject sessionButtonPrefab;

    private MyNetworkDiscovery networkDiscovery;
    private Dictionary<string, string> discoveredSessions = new Dictionary<string, string>();

    private bool isDiscovering = false;
    private float discoveryTimer = 0f;
    private float discoveryInterval = 1f;
    private float maxDiscoveryDuration = 10f;

    void Start()
    {
        networkDiscovery = GetComponent<MyNetworkDiscovery>();
        networkDiscovery.OnServerFound += OnServerFound;

        hostButton.onClick.AddListener(StartLANHost);
        joinButton.onClick.AddListener(StartLANClient);
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshDiscovery);
    }

    void OnDestroy()
    {
        if (networkDiscovery != null)
            networkDiscovery.OnServerFound -= OnServerFound;
    }

    public void StartLANHost()
    {
        networkManager.StartHost();
        networkDiscovery.StartServer();

     
    }

    public void StartLANClient()
    {
        RefreshDiscovery();
    }

    public void RefreshDiscovery()
    {
        discoveredSessions.Clear();
        foreach (Transform child in sessionListParent)
            Destroy(child.gameObject);

        if (!isDiscovering)
            networkDiscovery.StartClient();

        isDiscovering = true;
        discoveryTimer = 0f;
    }

    void Update()
    {
        if (!isDiscovering) return;

        discoveryTimer += Time.deltaTime;

        if (discoveryTimer >= discoveryInterval)
        {
            discoveryTimer = 0f;
            networkDiscovery.ClientBroadcast(new DiscoveryBroadcastData());
        }

        if (discoveryTimer >= maxDiscoveryDuration)
        {
            isDiscovering = false;
            networkDiscovery.StopDiscovery();
        }
    }

    public void Stop()
    {
        networkDiscovery.StopDiscovery();
        isDiscovering = false;
    }

    void OnServerFound(IPEndPoint sender, DiscoveryResponseData response)
    {
        if (discoveredSessions.ContainsKey(response.ServerAddress.ToString()))
            return;

        discoveredSessions.Add(response.ServerAddress.ToString(), sender.Address.ToString());

        GameObject buttonObj = Instantiate(sessionButtonPrefab, sessionListParent);
        buttonObj.GetComponentInChildren<TMP_Text>().text = response.ServerName.ToString();
        buttonObj.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            Stop();

            var transport = (Unity.Netcode.Transports.UTP.UnityTransport)networkManager.NetworkConfig.NetworkTransport;
            transport.SetConnectionData(sender.Address.ToString(), 7777);
            networkManager.StartClient();
        });

        Debug.Log($"? Found LAN server: {response.ServerName} at {sender.Address}");
    }
}
