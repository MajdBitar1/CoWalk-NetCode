using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepsAudio : MonoBehaviour
{
    [SerializeField] List<AudioClip> clips;
    [SerializeField] AudioSource audioSource;
    private int clipcounter = 0;

    private Vector3 StartingPosition, PrevPosition;

    [SerializeField] float stepDistance = 0.5f;
    private float timer = 0;
    private bool isStandingStill = false;

    private bool isLocal = false;

    private AuraManager m_auraManager;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartingPosition = transform.position;

        if (GetComponentInParent<NetworkObject>().IsOwner)
        {
            audioSource.outputAudioMixerGroup = GameManager.Instance.GetAudioMixer().FindMatchingGroups("LocalFootsteps")[0];
            isLocal = true;
            return;
        }
        m_auraManager = FindAnyObjectByType<FeedbackManager>().GetAuraEffect();
        if (m_auraManager == null)
        {
            Debug.LogError("[FOOTSTEPS] INIT No AuraManager found");
            return;
        }
        m_auraManager.OverridePeriod();
    }

    public void PlayFootstepSound()
    {
        AudioClip clip = clips[clipcounter];
        audioSource.clip = clip;
        audioSource.Play();
        clipcounter++;
        if (clipcounter >= clips.Count)
        {
            clipcounter = 0;
        }

        if (!isLocal)
        {
            if (m_auraManager != null)
            {
                m_auraManager.PlayAura();
            }
            else
            {
                Debug.LogError("[FOOTSTEPS] No AuraManager found");
                m_auraManager = FindAnyObjectByType<FeedbackManager>().GetAuraEffect();
            }
        }    

    }

    private void Update()
    {
        if (Vector3.Distance(StartingPosition, transform.position) > stepDistance)
        {
            timer = 0;
            PlayFootstepSound();
            StartingPosition = transform.position;
        }
        if (PrevPosition == transform.position && !isStandingStill)
        {
            timer = Time.deltaTime;
            if (timer >= 2f)
            {
                timer = 0;
                StartingPosition = transform.position;
                isStandingStill = true;
            }
        }
        PrevPosition = transform.position;
    }
}
