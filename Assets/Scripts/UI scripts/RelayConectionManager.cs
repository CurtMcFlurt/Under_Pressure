using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System;
using Unity.Services.Multiplayer;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelayConnectionManager : MonoBehaviour
{
    public Camera throwAwayCamera;
    [SerializeField] int m_MaxPlayers = 4;

    [Header("UI References")]
    public TMP_InputField joinCodeInput;
    public TMP_InputField NameInput;
    public TMP_Text joinCodeDisplay;
    public GameObject uiPanel;

    [Header("Connection Mode Buttons")]
    public Button relayConnectButton;
    public Button lanConnectButton;
    public Button lanHostButton;

    [Header("Netcode")]
    public NetworkManager m_NetworkManager;
    public LANDiscoveryManager lanDiscoveryManager;

    [Header("Mode Panels")]
    public GameObject relayPanel;
    public GameObject lanPanel;

    private ISession m_Session;

    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    public enum ConnectionMode
    {
        None,
        Relay,
        LAN
    }

    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
    public ConnectionMode Mode { get; private set; } = ConnectionMode.None;

    async void Awake()
    {
        m_NetworkManager = FindFirstObjectByType<NetworkManager>();
        m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;

        await UnityServices.InitializeAsync();

        // Hook button click events
        relayConnectButton.onClick.AddListener(() => StartRelayConnection());
        lanConnectButton.onClick.AddListener(() => StartLANHostConnection());
        lanHostButton.onClick.AddListener(() => StartLAN());
    }

    public async void Disconnect()
    {
        if (m_Session != null)
        {
            await m_Session.LeaveAsync();
        }

        throwAwayCamera.enabled = true;
        State = ConnectionState.Disconnected;

        if (m_NetworkManager != null && m_NetworkManager.IsConnectedClient)
        {
            m_NetworkManager.Shutdown();
        }

        SceneManager.LoadScene("CoopTestScene");
        Destroy(m_NetworkManager.gameObject);
    }

    public void StartRelayConnection()
    {
        if (Mode == ConnectionMode.LAN)
        {
            lanDiscoveryManager.Stop(); // <--- Safely stop LAN discovery
        }

        Mode = ConnectionMode.Relay;
        relayPanel.SetActive(true);
        lanPanel.SetActive(false);

        StartRelay();
    }


    public void StartLANHostConnection()
    {
        if (m_NetworkManager.IsListening)
            m_NetworkManager.Shutdown();

        Mode = ConnectionMode.LAN;
        relayPanel.SetActive(false);
        lanPanel.SetActive(true);

        var transport = (UnityTransport)m_NetworkManager.NetworkConfig.NetworkTransport;
        transport.SetConnectionData("0.0.0.0", 7777, "0.0.0.0");

        lanDiscoveryManager.StartLANHost();
    }
    public async void StartRelay()
    {
        await CreateOrJoinSessionAsync(joinCodeInput.text, NameInput.text);
    }

    public void StartLAN()
    {
        lanDiscoveryManager.StartLANClient(); // or StartLANHost() based on how you want to test
    }

    public async Task CreateOrJoinSessionAsync(string sessionName, string profileName)
    {
        if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(sessionName))
        {
            Debug.LogError("Please provide a player and session name, to login.");
            return;
        }

        State = ConnectionState.Connecting;
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SwitchProfile(profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = m_MaxPlayers
            }.WithDistributedAuthorityNetwork();

            m_Session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);
            State = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            State = ConnectionState.Disconnected;
            Debug.LogException(e);
        }
    }

    void OnClientConnectedCallback(ulong clientId)
    {
        if (m_NetworkManager.LocalClientId == clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
            DisableUnecesaries();
        }
    }

    void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (m_NetworkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{m_NetworkManager.LocalClientId} is the session owner!");
            DisableUnecesaries();
        }
    }

    public void DisableUnecesaries()
    {
        throwAwayCamera.enabled = false;

        uiPanel.SetActive(false);
        relayPanel.SetActive(false);
        lanPanel.SetActive(false);
    }
}
