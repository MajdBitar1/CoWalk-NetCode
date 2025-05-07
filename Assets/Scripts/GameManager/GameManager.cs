using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.MultiplayerBlocks.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.CullingGroup;

public class GameManager : Singleton<GameManager>
{
    public delegate void PlayerListUpdated();
    public static List<GameObject> PlayerRefList = new List<GameObject>();
    public static GameObject origin, LocalPlayerObject;
    public static GameObject RemotePlayerObject;
    public static GameObject Experimenter, Participant;
    public static bool PlayersReady = false;

    [SerializeField] private AudioMixer AudioMixer;

    [SerializeField] TracingSetup Tracer;

    public int previousPlayerCount = 0;

    public NetworkVariable<bool> AuraState = new NetworkVariable<bool>();
    public NetworkVariable<bool> RhythmState = new NetworkVariable<bool>();
    public NetworkVariable<bool> TracingState = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize the conditions to false
            AuraState.Value = false;
            RhythmState.Value = false;
            TracingState.Value = false;
        }
        else
        {
            AuraState.OnValueChanged += ConditionChanged;
            RhythmState.OnValueChanged += ConditionChanged;
            TracingState.OnValueChanged += ConditionChanged;
            ApplyConditionChange();
        }
    }

    public override void OnNetworkDespawn()
    {
        AuraState.OnValueChanged -= ConditionChanged;
        RhythmState.OnValueChanged -= ConditionChanged;
        TracingState.OnValueChanged -= ConditionChanged;

        // Clean up the PlayerRefList when the GameManager is destroyed
        PlayerRefList.Clear();
        AudioMixer.FindSnapshot("NoFoot").TransitionTo(0.1f);
    }

    void FixedUpdate()
    {
        CheckPlayerCount();
    }

    void CheckPlayerCount()
    {
        AvatarEntity[] avatarentities = FindObjectsByType<AvatarEntity>(FindObjectsSortMode.None);//GameObject.FindGameObjectsWithTag("Player");
        //get the GameObjects of the AvatarEntity objects and put them in an array
        GameObject[] players = new GameObject[avatarentities.Length];
        for (int i = 0; i < avatarentities.Length; i++)
        {
            players[i] = avatarentities[i].gameObject;
        }

        int count = players.Count();

        if (count != previousPlayerCount)
        {
            previousPlayerCount = count;
            PlayerRefList = new List<GameObject>(players);
            DefineLocalPlayer();
        }
    }
    void DefineLocalPlayer()
    {
        for (int i = 0; i < PlayerRefList.Count; i++)
        {
            PlayerRefList[i].gameObject.layer = 6;

            if (PlayerRefList[i].GetComponent<AvatarEntity>().IsLocal)
            {
                LocalPlayerObject = PlayerRefList[i];
                Debug.Log("[GM] Local Player Defined: " + LocalPlayerObject.name);
            }
            else
            {
                RemotePlayerObject = PlayerRefList[i];
                Debug.Log("[GM] Remote Player Defined: " + RemotePlayerObject.name);
            }
        }
    }

    public void DefineExperimenter(GameObject playerobj)
    {
        Debug.Log("[GM] Experimenter Defined: " + LocalPlayerObject.name);
        Experimenter = LocalPlayerObject;
    }

    public void DefineParticipant(GameObject playerobj)
    {
        Debug.Log("[GM] Participant Defined: " + LocalPlayerObject.name);
        Participant = LocalPlayerObject;
    }
    public int TryUpdateConditions(bool aura, bool rhythm, bool tracing)
    {
        if (Experimenter == null || Participant == null)
        {
            return -1;
        }
        UpdateConditionsServerRpc(aura, rhythm, tracing);
        return 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateConditionsServerRpc(bool aura, bool rhythm, bool tracing)
    {
        AuraState.Value = aura;  // Direct server-side modification
        RhythmState.Value = rhythm;
        TracingState.Value = tracing;
        ApplyConditionChange();
    }


    private void ConditionChanged(bool previous, bool current)
    {
        ApplyConditionChange();
    }
    private void ApplyConditionChange()
    {
        //Enable Aura
        if (RemotePlayerObject != null)
        {
            RemotePlayerObject.GetComponentInChildren<AuraManager>().SetAuraState(AuraState.Value);
            Debug.Log("[GM] Aura set to" + AuraState.Value);
        }

        if (RhythmState.Value)
        {
            //Enable Rhythm
            AudioMixer.FindSnapshot("Default").TransitionTo(0.1f);
            Debug.Log("[GM] Rhythm Enabled");
        }
        else
        {
            //Disable Rhythm
            AudioMixer.FindSnapshot("NoFoot").TransitionTo(0.1f);
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
}
