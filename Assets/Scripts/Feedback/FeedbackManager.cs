using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

public class FeedbackManager : NetworkBehaviour
{
    public NetworkVariable<bool> AuraState = new NetworkVariable<bool>();
    public NetworkVariable<bool> RhythmState = new NetworkVariable<bool>();
    public NetworkVariable<bool> TracingState = new NetworkVariable<bool>();
    public NetworkVariable<bool> ActivateOnLocalPlayer = new NetworkVariable<bool>();

    [SerializeField] TracingSetup Tracer;

    private AuraManager[] AuraManagers;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize the conditions to false
            AuraState.Value = false;
            RhythmState.Value = false;
            ActivateOnLocalPlayer.Value = false;
            TracingState.Value = false;
        }
        else
        {
            AuraState.OnValueChanged += ConditionChanged;
            RhythmState.OnValueChanged += ConditionChanged;
            TracingState.OnValueChanged += ConditionChanged;
            ActivateOnLocalPlayer.OnValueChanged += ConditionChanged;

            ApplyConditionChange();
        }

        if (Tracer == null)
        {
            Tracer = FindObjectOfType<TracingSetup>();
            if (Tracer == null)
            {
                Debug.LogError("[GM] Tracer not found in the scene.");
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        AuraState.OnValueChanged -= ConditionChanged;
        RhythmState.OnValueChanged -= ConditionChanged;
        TracingState.OnValueChanged -= ConditionChanged;

    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateConditionsServerRpc(bool aura, bool rhythm, bool tracing)
    {

        AuraState.Value = aura;  // Direct server-side modification
        RhythmState.Value = rhythm;
        TracingState.Value = tracing;
        ApplyConditionChange();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateSelfAuraServerRpc(bool State)
    {
        ActivateOnLocalPlayer.Value = State;  // Direct server-side modification
        ApplyConditionChange();
    }

    public int TryUpdateConditions(bool aura, bool rhythm, bool tracing)
    {
        if (GameManager.Experimenter == null || GameManager.Participant == null)
        {
            Debug.LogError("[GM] CHANGES REVERTED: Experimenter or Participant is null");
            return -1;
        }
        UpdateConditionsServerRpc(aura, rhythm, tracing);
        return 0;
    }

    private void ConditionChanged(bool previous, bool current)
    {
        ApplyConditionChange();
    }

    [ContextMenu("Apply Condition Change")]
    public void Demo()
    {
        UpdateConditionsServerRpc(true, false, false);
        UpdateSelfAuraServerRpc(true);
        ApplyConditionChange();
    }
    private void ApplyConditionChange()
    {
        UpdateAllAuraManagers();
        if (RhythmState.Value)
        {
            //Enable Rhythm
            GameManager.Instance.GetAudioMixer().FindSnapshot("Default").TransitionTo(0.1f);
            Debug.Log("[GM] Rhythm Enabled");
        }
        else
        {
            //Disable Rhythm
            GameManager.Instance.GetAudioMixer().FindSnapshot("NoFoot").TransitionTo(0.1f);
            Debug.Log("[GM] Rhythm Disabled");
        }

        if (TracingState.Value)
        {
            //Enable Tracing
            Tracer.InitiateTracing();
            Debug.Log("[GM] Tracing Enabled");
        }
        else
        {
            //Disable Tracing
            Tracer.EndTracing();
            Debug.Log("[GM] Tracing Disabled");
        }
    }

    private void UpdateAllAuraManagers()
    {
        AuraManagers = FindObjectsByType<AuraManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (AuraManager auraManager in AuraManagers)
        {
            if (auraManager != null)
            {
                auraManager.AuraState = AuraState.Value;
                auraManager.activateOnLocalPlayer = ActivateOnLocalPlayer.Value;
            }
            else
            {
                Debug.LogError("[GM] AuraManager not found in the scene.");
            }
        }
    }
}
