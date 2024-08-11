using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;
using UnityEditor;

public class TestRelay : MonoBehaviour
{
    public TMP_InputField joinCodeTextBox;
    public TextMeshProUGUI thisServersCode;
    public GameObject startGameUI;

    // Start is called before the first frame update
    async void Start()
    {
        startGameUI.SetActive(true);

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(9);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            EditorGUIUtility.systemCopyBuffer = joinCode;

            thisServersCode.text = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            startGameUI.SetActive(false);
        }
        catch(RelayServiceException e){
            Debug.Log(e);
        }

    }

    public async void JoinRelay(string joinCode)
    {
        joinCode = joinCodeTextBox.text;

        thisServersCode.text = joinCode;

        try
        {
            Debug.Log("Joining with " + joinCode);
            await RelayService.Instance.JoinAllocationAsync(joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            startGameUI.SetActive(false);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
