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

    [SerializeField] int MaxRange = 15;
    [SerializeField] int ConvRange = 5;
    [SerializeField] float RollOff = 1f;

    bool channelReady = false;

    float Timer = 0f;
    private async void Start()
    {
        await StartUnityServices();  // Make sure Unity Services are initialized before anything else
    }

    async Task StartUnityServices()
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

    async Task InitializeVivoxAndJoinChannel()
    {
        try
        {
            await VivoxService.Instance.InitializeAsync();
            await VivoxService.Instance.LoginAsync();

            Debug.Log("[VIVOX] Vivox initialized and logged in");

            Channel3DProperties channel3DProperties = new Channel3DProperties(
                MaxRange,    // audibility range
                ConvRange,     // conversational range
                RollOff,     // audio roll-off
                AudioFadeModel.ExponentialByDistance // audio fade model
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

    async void Update()
    {
        if (GameManager.LocalPlayerObject == null || GameManager.RemotePlayerObject)
        {
            return;
        }
        if (!channelReady)
        {
            Timer += Time.deltaTime;
            if (Timer >= 10f)
            {
                Timer = 0f;
                await StartUnityServices();
            }
            return;
        }

        Transform remotePlayerTrans = GameManager.RemotePlayerObject.transform;
        Transform localPlayerTrans = GameManager.LocalPlayerObject.transform;

        VivoxService.Instance.Set3DPosition(
            localPlayerTrans.position, // speaker position
            remotePlayerTrans.position, // listener position
            remotePlayerTrans.forward, // listener at orientation
            remotePlayerTrans.up, // listener up orientation
            channelToJoin
        );
    }
}
