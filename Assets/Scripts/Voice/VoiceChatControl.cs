using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Unity.Netcode;
using System.Threading.Tasks;

public class VoiceChatControl : MonoBehaviour
{
    [SerializeField] string channelToJoin = "Lobby";
    [SerializeField] int maxRange = 15;
    [SerializeField] int convRange = 5;
    [SerializeField] float rollOff = 1f;
    [SerializeField] GameObject origin;

    bool channelReady = false;
    int counter = 0;

    async void Start()
    {
        await StartUnityServices();
        await InitializeVivoxAndJoinChannel();
        await CoonectToChannel();
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
    }
    async Task InitializeVivoxAndJoinChannel()
    {
        try
        {
            await VivoxService.Instance.InitializeAsync();
            await VivoxService.Instance.LoginAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[VIVOX] Vivox initialization failed: {e.Message}");
        }
    }

    async Task CoonectToChannel()
    {
        try
        {
            Channel3DProperties channel3DProperties = new Channel3DProperties(
                maxRange,
                convRange,
                rollOff,
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

    async Task CheckAudio()
    {
        switch (counter)
        {
            case 0:
                // No action
                break;
            case 1:
                await UnityServices.InitializeAsync();
                break;
            case 2:
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                break;
            case 3:
                await InitializeVivoxAndJoinChannel();
                break;
            case 4:
                await VivoxService.Instance.InitializeAsync();
                break;
            case 5:
                await VivoxService.Instance.LoginAsync();
                break;
            case 6:
                await CoonectToChannel();
                break;
            default:
                // Optionally reset or clamp the counter
                counter = 0;
                break;
        }
    }

    [ContextMenu("ForceAudio")]
    public async void ForceAudio()
    {
        counter++;
        await CheckAudio();
    }

    private async void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.One))
        {
            counter++;
            await CheckAudio();
            return;
        }

        if (origin == null)
        {
            Debug.LogWarning("[VIVOX] Origin GameObject is not assigned.");
            return;
        }
        if (!channelReady)
        {
            return;
        }
        VivoxService.Instance.Set3DPosition(
            origin,
            channelToJoin
        );
    }
    //private void SetFull3DPosition()
    //{
    //    if (GameManager.LocalPlayerObject == null || GameManager.RemotePlayerObject == null)
    //        return;

    //    Transform localPlayerTrans = GameManager.LocalPlayerObject.transform;
    //    Transform remotePlayerTrans = GameManager.RemotePlayerObject.transform;

    //    VivoxService.Instance.Set3DPosition(
    //        localPlayerTrans.position,
    //        remotePlayerTrans.position,
    //        remotePlayerTrans.forward,
    //        remotePlayerTrans.up,
    //        channelToJoin
    //    );
    //}
}