using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Unity.Netcode;
using System.Collections;

public class VoiceChatControl : MonoBehaviour
{
    private bool AttemptingToJoin = false;
    private void Awake()
    {
        InitializeAsync();
    }

    private void Start()
    {
        StartCoroutine(Initialize());
    }

    private void FixedUpdate()
    {
        if (VivoxService.Instance.ActiveChannels.Count < 1 && !AttemptingToJoin)
        {
            Initialize();
        }

    }

    private IEnumerator Initialize()
    {
        AttemptingToJoin = true;
        //Wait 5 seconds
        yield return new WaitForSeconds(10);
        InitializeAsync();
        yield return new WaitForSeconds(2);
        LoginToVivoxAsync();
        yield return new WaitForSeconds(2);
        JoinChannelAsync();
        yield return null;
    }

    [ContextMenu("Initialize")]
    async void InitializeAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await VivoxService.Instance.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[VOICECHAT]Unity Services Initialization failed: {e.Message}");
            return;
        }
    }

    [ContextMenu("Login to Vivox")]
    public async void LoginToVivoxAsync()
    {
        try
        {
            await VivoxService.Instance.InitializeAsync();
            LoginOptions options = new LoginOptions();
            options.EnableTTS = false;
            await VivoxService.Instance.LoginAsync(options);
        }
        catch (Exception e)
        {
            Debug.LogError($"[VOICECHAT]Unity Services Initialization failed: {e.Message}");
            return;
        }
    }

    [ContextMenu("Join Channel Vivox")]
    public async void JoinChannelAsync()
    {
        try
        {
            await VivoxService.Instance.InitializeAsync();
            string channelToJoin = "Lobby";
            Channel3DProperties channel3DProperties = new Channel3DProperties(
                15,
                6,
                1f,
                AudioFadeModel.LinearByDistance
            );
            //await VivoxService.Instance.JoinPositionalChannelAsync(channelToJoin, ChatCapability.AudioOnly, channel3DProperties);
            await VivoxService.Instance.JoinGroupChannelAsync(channelToJoin, ChatCapability.AudioOnly);
            AttemptingToJoin = false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[VOICECHAT]Unity Services Initialization failed: {e.Message}");
            AttemptingToJoin = false;
            return;
        }
    }
}
