using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

public class FeedbackManager : NetworkBehaviour
{
    public NetworkVariable<bool> TracingState = new NetworkVariable<bool>();

    public NetworkVariable<int> StateDefined = new NetworkVariable<int>();

    [SerializeField] TracingSetup Tracer;

    [SerializeField] GameObject GuidingArrowEffect;

    [SerializeField] AuraManager AuraEffect;

    private GameObject OtherPlayer;

    private int prevValue;
    private bool prevTracing;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize the conditions to false
            StateDefined.Value = 0;
            TracingState.Value = false;
            prevValue = 0;
        }
        else
        {
            StateDefined.OnValueChanged += ExperimentStateChanged;
            TracingState.OnValueChanged += TracingStateUpdated;
            DeactivateAllStates();
            prevValue = 0;
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
        StateDefined.OnValueChanged -= ExperimentStateChanged;
        TracingState.OnValueChanged -= TracingStateUpdated;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateExperimentStateServerRpc(int newState)
    {
        StateDefined.Value = newState;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateTracingStateServerRpc(bool tracing)
    {
        TracingState.Value = tracing;
    }

    public void PlayFeedback(int counter)
    {
        switch (StateDefined.Value)
        {
            case 0:
                // No effect
                break;
            case 1:
                if (counter == 0)
                {
                    AuraFeedback();
                }
                break;
            case 2:
                GuidingArrowFeedback();
                break;
            default:
                Debug.LogError("[GM] Invalid state: " + StateDefined.Value);
                break;
        }
    }

    private void AuraFeedback()
    {
        if (AuraEffect == null)
        {
            AuraEffect = OtherPlayer.GetComponentInChildren<AuraManager>();
        }
        AuraEffect.PlayAura();
    }

    private void GuidingArrowFeedback()
    {
        GuidingArrowEffect.GetComponent<GuidingArrowManager>().ShowArrow();
    }

    private void LateUpdate()
    {
        if (OtherPlayer == null)
        {
            OtherPlayer = GameManager.RemotePlayerObject;
        }
        if (prevValue != StateDefined.Value)
        {
            ApplyStateChange();
        }
        if (prevTracing != TracingState.Value)
        {
            Debug.Log("[TRACING] Tracing State Change Detected Inside LateUpdate");
            UpdateTracing();
        }
        prevValue = StateDefined.Value;
        prevTracing = TracingState.Value;
    }

    private void ApplyStateChange()
    {
        if (OtherPlayer == null)
        {
            return;
        }

        //Disable all Effects
        DeactivateAllStates();
        //Enable the Effect that corresponds to the current state
        switch(StateDefined.Value)
        {
            case 0:
                // No effect
                break;
            case 1:
                ActivateAura();
                break;
            case 2:
                ActivateGuidingArrow();
                break;
            default:
                Debug.LogError("[GM] Invalid state: " + StateDefined.Value);
                break;
        }
    }
    private void ActivateGuidingArrow()
    {
        GuidingArrowEffect.SetActive(true);
        GuidingArrowEffect.GetComponent<GuidingArrowManager>().SetOtherPlayer(OtherPlayer);
        Debug.Log("[GM] Guiding arrow activated");
    }

    private void ActivateAura()
    {
        if (AuraEffect == null)
        {
            AuraEffect = OtherPlayer.GetComponentInChildren<AuraManager>();
        }
        AuraEffect.GetComponent<AuraManager>().SetOtherPlayer(GameManager.LocalPlayerObject);
        AuraEffect.isActive = true;
        Debug.Log("[GM] Aura activated");
    }

    private void DeactivateAllStates()
    {
        GuidingArrowEffect.SetActive(false);
        if (AuraEffect != null)
        {
            AuraEffect.isActive = false;
        }
    }

    private void TracingStateUpdated(bool previous, bool current)
    {
        Debug.Log("[TRACING] Tracing State Change Detected EVENT");
        TracingState.Value = current;
        UpdateTracing();
    }

    private void UpdateTracing()
    {
        if (TracingState.Value)
        {
            Tracer.InitiateTracing();
        }
        else
        {
            Tracer.EndTracing();
        }
    }

    private void ExperimentStateChanged(int prev, int current)
    {
        StateDefined.Value = current;
        ApplyStateChange();
    }
}
