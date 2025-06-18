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
    public static List<GameObject> PlayerRefList = new List<GameObject>();
    public static GameObject origin;
    public static GameObject LocalPlayerObject, RemotePlayerObject;
    public static GameObject Experimenter;

    private int previousPlayerCount = 0;

    [SerializeField] private AudioMixer AudioMixer;

    public override void Awake()
    {
        base.Awake();
        Application.runInBackground = true;
    }
    public override void OnNetworkDespawn()
    {
        PlayerRefList.Clear();
    }

    private void FixedUpdate()
    {
        CheckPlayerCount();
    }
    void CheckPlayerCount()
    {
        AvatarEntity[] avatarentities = FindObjectsByType<AvatarEntity>(FindObjectsSortMode.None);
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
    }

    public AudioMixer GetAudioMixer()
    {
        return AudioMixer;
    }

}
