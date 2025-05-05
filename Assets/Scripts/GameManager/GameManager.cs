using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.MultiplayerBlocks.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : Singleton<GameManager>
{
    public delegate void PlayerListUpdated();
    public static List<GameObject> PlayerRefList = new List<GameObject>();
    public static GameObject origin, LocalPlayerObject;
    public static GameObject RemotePlayerObject;
    public static GameObject Experimenter, Participant;
    public static bool PlayersReady = false;

    [SerializeField] private GameObject localOrigin;

    [SerializeField] private AudioMixer AudioMixer;

    public int previousPlayerCount = 0;

    public NetworkVariable<bool> AuraState = new NetworkVariable<bool>();
    public NetworkVariable<bool> RhythmState = new NetworkVariable<bool>();
    public NetworkVariable<bool> TracingState = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        //origin = GameObject.Find("origin");
        //if (origin == null)
        //{
        //    Debug.LogError("[GM] Origin not found!");
        //}

        //localOrigin = FindFirstObjectByType<PlayerDataTransmitter>().gameObject;
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
            Debug.Log("[GM] Player List Updated, Count: " + count);
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
                //Debug
                //REMOVE THIS LATER
                //Experimenter = LocalPlayerObject;
                Debug.Log("[GM] Local Player Defined: " + LocalPlayerObject.name);
            }
            else
            {
                RemotePlayerObject = PlayerRefList[i];
                //Participant = RemotePlayerObject;
                if (RemotePlayerObject.GetComponentInChildren<AuraManager>() == null)
                {
                    //GameObject aura = Instantiate(AuraVisual, RemotePlayerObject.transform);
                    //aura.transform.localPosition = new Vector3(0, 0, 0);
                }
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

    [ContextMenu("Set Aura")]
    public void DEMO()
    {
        UpdateConditionsServerRpc(true, true, false);
    }
    [ServerRpc]
    public void UpdateConditionsServerRpc(bool aura, bool rhythm, bool tracing)
    {
        AuraState.Value = aura;  // Direct server-side modification
        RhythmState.Value = rhythm;
        TracingState.Value = tracing;
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
            Debug.Log("[GM] Tracing Enabled");
        }
        else
        {
            //Disable Tracing
            Debug.Log("[GM] Tracing Disabled");
        }
    }

    private void OnDisable()
    {
        // Clean up the PlayerRefList when the GameManager is destroyed
        PlayerRefList.Clear();
        AudioMixer.FindSnapshot("NoFoot").TransitionTo(0.1f);
    }

    public GameObject GetOrigin()
    {
        return localOrigin;
    }
}
