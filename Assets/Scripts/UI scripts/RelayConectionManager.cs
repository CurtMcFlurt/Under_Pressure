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





public class RelayConnectionManager : MonoBehaviour
{
    public Camera throwAwayCamera;
    [SerializeField]
    int m_MaxPlayers = 4;
    [Header("UI References")]
    public TMP_InputField joinCodeInput;
    public TMP_InputField NameInput;
    public TMP_Text joinCodeDisplay;
    public GameObject uiPanel;
    ISession m_Session;
    [Header("Netcode")]
    public NetworkManager m_NetworkManager; // Assign your scene's NetworkManager here
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    async void Awake()
    {
        // Find the NetworkManager in the Scene
        m_NetworkManager = FindFirstObjectByType<NetworkManager>();
        m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        await UnityServices.InitializeAsync();
    }

    public async void Disconnect()
    {
        if (m_Session != null)
        {
            await m_Session.LeaveAsync();
        }
        throwAwayCamera.enabled = true;
        State = ConnectionState.Disconnected;
    }

    public void startConnection() => CreateOrJoinSessionAsync(joinCodeInput.text, NameInput.text);
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
            // Only sign in if not already signed in.
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SwitchProfile(profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            // Set the session options.
            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = m_MaxPlayers
            }.WithDistributedAuthorityNetwork();

            // Join a session if it already exists, or create a new one.
            m_Session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);
            State = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            State = ConnectionState.Disconnected;
            Debug.LogException(e);
        }
    }
    // Just for logging.
    void OnClientConnectedCallback(ulong clientId)
    {
        if (m_NetworkManager.LocalClientId == clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
            throwAwayCamera.enabled = false;
            uiPanel.gameObject.SetActive(false);
        }
    }

    // Just for logging.
    void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (m_NetworkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{m_NetworkManager.LocalClientId} is the session owner!");
            throwAwayCamera.enabled = false;

            uiPanel.gameObject.SetActive(false);
        }
    }
}
