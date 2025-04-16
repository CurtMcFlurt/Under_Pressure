using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class RelayConnectionManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField joinCodeInput;
    public TMP_Text joinCodeDisplay;
    public GameObject uiPanel;

    [Header("Netcode")]
    public NetworkManager netManager; // Assign your scene's NetworkManager here

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void HostGame()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code: " + joinCode);
            if (joinCodeDisplay != null) joinCodeDisplay.text = joinCode;

            var relayServerData = new RelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData,
                true
            );

            var transport = netManager.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            if (netManager.StartHost())
            {
                Debug.Log("Host started successfully");
                if (uiPanel != null) uiPanel.SetActive(false);
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay host error: " + e.Message);
        }
    }

    public async void JoinGame()
    {
        try
        {
            string joinCode = joinCodeInput.text.Trim();
            if (string.IsNullOrEmpty(joinCode)) return;

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var relayServerData = new RelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                true
            );

            var transport = netManager.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            if (netManager.StartClient())
            {
                Debug.Log("Client started successfully");
                if (uiPanel != null) uiPanel.SetActive(false);
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay join error: " + e.Message);
        }
    }
}
