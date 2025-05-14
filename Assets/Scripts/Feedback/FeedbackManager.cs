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

    [SerializeField] GameObject PostProcessingGrayEffect;

    [SerializeField] GameObject BlinkingLightEffect;

    [SerializeField] GameObject GuidingArrowEffect;

    [SerializeField] AuraManager AuraEffect;

    private GameObject OtherPlayer;

    private int prevValue;

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
            TracingState.OnValueChanged += ConditionChanged;
            StateDefined.OnValueChanged += ExperimentStateChanged;
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
        TracingState.OnValueChanged -= ConditionChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateExperimentStateServerRpc(int newState)
    {
        StateDefined.Value = newState;
    }

    private void LateUpdate()
    {
        if (prevValue != StateDefined.Value)
        {
            ApplyStateChange();
        }
        prevValue = StateDefined.Value;
    }

    private void ApplyStateChange()
    {
        if (OtherPlayer == null)
        {
            OtherPlayer = GameManager.RemotePlayerObject;
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
                ActivateBlinkingLight();
                break;
            case 3:
                ActivateGuidingArrow();
                break;
            case 4:
                //ActivatePPGray();
                break;
            default:
                Debug.LogError("[GM] Invalid state: " + StateDefined.Value);
                break;
        }
    }

    private void ActivatePPGray()
    {
        PostProcessingGrayEffect.SetActive(true);
        PostProcessingGrayEffect.GetComponent<ColorInGrayOut>().SetOtherPlayer(OtherPlayer);
        Debug.Log("[GM] PostProcessing Gray activated");
    }

    private void ActivateBlinkingLight()
    {
        BlinkingLightEffect.SetActive(true);
        BlinkingLightEffect.GetComponent<PositionOtherToFOV>().SetOtherPlayer(OtherPlayer);
        Debug.Log("[GM] Blinking light activated");
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
        AuraEffect.isActive = true;
        AuraEffect.GetComponent<AuraManager>().SetOtherPlayer(GameManager.LocalPlayerObject);
        Debug.Log("[GM] Aura activated");
    }

    private void DeactivateAllStates()
    {
        if (AuraEffect == null)
        {
            AuraEffect = OtherPlayer.GetComponentInChildren<AuraManager>();
        }
        PostProcessingGrayEffect.SetActive(false);
        BlinkingLightEffect.SetActive(false);
        GuidingArrowEffect.SetActive(false);
        AuraEffect.isActive = false;
        Debug.Log("[GM] All effects deactivated");
    }

    public int TryUpdateConditions(bool tracing)
    {
        if (GameManager.Experimenter == null || GameManager.Participant == null)
        {
            Debug.LogError("[GM] CHANGES REVERTED: Experimenter or Participant is null");
            return -1;
        }
        UpdateConditionsServerRpc(tracing);
        return 0;
    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdateConditionsServerRpc(bool tracing)
    {
        TracingState.Value = tracing;
    }

    private void ConditionChanged(bool previous, bool current)
    {
    }

    private void ExperimentStateChanged(int prev, int current)
    {
        StateDefined.Value = current;
        ApplyStateChange();
        //if (current != prev)
        //{
        //    ApplyStateChange();
        //}
    }
}
