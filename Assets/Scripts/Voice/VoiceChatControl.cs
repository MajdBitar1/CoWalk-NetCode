using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Unity.Netcode;
using System.Collections;
using System.Threading.Tasks;

public class VoiceChatControl : MonoBehaviour
{
    [SerializeField] string channelToJoin = "Lobby";

    bool channelReady = false;
    private async void Start()
    {
        await StartUnityServices();  // Make sure Unity Services are initialized before anything else
    }

    private async Task StartUnityServices()
    {
        try
        {
            if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
            {
                await UnityServices.InitializeAsync();
            }
            else
            {
                Debug.Log("[VIVOX] Unity Services already initialized");
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                Debug.Log("[VIVOX] Already signed in to Unity Services");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[VIVOX] Unity Services Initialization failed: {e.Message}");
        }

        try
        {
            Debug.Log("[VIVOX] Unity Services initialized and authenticated");
            // Now it's safe to start Vivox
            await InitializeVivoxAndJoinChannel();
        }
        catch (Exception e)
        {
            Debug.LogError($"[VIVOX] Vivox initialization failed: {e.Message}");
        }
    }

    private async Task InitializeVivoxAndJoinChannel()
    {
        try
        {
            await VivoxService.Instance.InitializeAsync();
            await VivoxService.Instance.LoginAsync();

            Debug.Log("[VIVOX] Vivox initialized and logged in");

            Channel3DProperties channel3DProperties = new Channel3DProperties(
                15,    // audibility range
                6,     // conversational range
                1f,     // audio roll-off
                AudioFadeModel.LinearByDistance
            );

            await VivoxService.Instance.JoinPositionalChannelAsync(
                channelToJoin,
                ChatCapability.AudioOnly,
                channel3DProperties
            );

            channelReady = true;
            Debug.Log("[VIVOX] Successfully joined positional audio channel");
        }
        catch (Exception e)
        {
            Debug.LogError($"[VIVOX] Vivox setup failed: {e.Message}");
        }
    }

    private void LateUpdate()
    {
        if (GameManager.LocalPlayerObject == null)
        {
            return;
        }
        if (!channelReady)
        {
            return;
        }

        VivoxService.Instance.Set3DPosition(
            GameManager.LocalPlayerObject,  // Player's position
            channelToJoin
        );
    }
}
