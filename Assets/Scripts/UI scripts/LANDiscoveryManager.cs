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
    public Transform sessionListParent;
    public GameObject sessionButtonPrefab;

    private MyNetworkDiscovery networkDiscovery;
    private Dictionary<string, string> discoveredSessions = new Dictionary<string, string>();

    void Start()
    {
        networkDiscovery = GetComponent<MyNetworkDiscovery>();
        networkDiscovery.OnServerFound += OnServerFound;

        hostButton.onClick.AddListener(StartLANHost);
        joinButton.onClick.AddListener(StartLANClient);
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

    private float discoveryInterval = 1f;
    private float discoveryDuration = 10f;
    private float elapsed = 0f;
    private bool isDiscovering = false;

    public void StartLANClient()
    {
        discoveredSessions.Clear();
        foreach (Transform child in sessionListParent)
            Destroy(child.gameObject);

        networkDiscovery.StartClient();
        isDiscovering = true;
        elapsed = 0f;
    }

    void Update()
    {
        if (!isDiscovering) return;

        elapsed += Time.deltaTime;
        if (elapsed >= discoveryDuration)
        {
            isDiscovering = false;
            return;
        }

        if (Time.frameCount % Mathf.RoundToInt(discoveryInterval / Time.deltaTime) == 0)
        {
            networkDiscovery.ClientBroadcast(new DiscoveryBroadcastData());
        }
    }

    public void Stop()
    {
        networkDiscovery.StopDiscovery();
    }
    void OnServerFound(IPEndPoint sender, DiscoveryResponseData response)
    {
        Debug.Log("ShouldFindServer");
        if (!discoveredSessions.ContainsKey(response.ServerAddress.ToString()))
        {
            discoveredSessions.Add(response.ServerAddress.ToString(), sender.Address.ToString());
            Debug.Log("foundServer");
            GameObject buttonObj = Instantiate(sessionButtonPrefab, sessionListParent);
            buttonObj.GetComponentInChildren<TMP_Text>().text = response.ServerName.ToString();
            buttonObj.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                // Stop listening/receiving before changing transport or starting client
                networkDiscovery.StopDiscovery();

                var transport = (Unity.Netcode.Transports.UTP.UnityTransport)networkManager.NetworkConfig.NetworkTransport;
                transport.SetConnectionData(sender.Address.ToString(), 7777);
                networkManager.StartClient();
            });
        }
    }
}
