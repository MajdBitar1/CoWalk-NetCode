using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepsAudio : MonoBehaviour
{
    [SerializeField] List<AudioClip> clips;
    [SerializeField] AudioSource audioSource;

    private Vector3 StartingPosition, PrevPosition;

    private int clipcounter = 0;
    private float timer = 0;


    [SerializeField] float stepDistance = 1f;
    private float StandingStillThreshold = 3f;

    private bool isStandingStill = false;
    private bool isLocal = false;

    private FeedbackManager m_feedback;

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

        m_feedback = FindAnyObjectByType<FeedbackManager>();
        if (m_feedback == null)
        {
            Debug.LogError("[FootstepsAudio] FeedbackManager not found in the scene.");
        }
        else
        {
            audioSource.outputAudioMixerGroup = GameManager.Instance.GetAudioMixer().FindMatchingGroups("OtherFootsteps")[0];
        }

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
            if (m_feedback == null)
            {
                m_feedback = FindAnyObjectByType<FeedbackManager>();
                return;
            }
            m_feedback.PlayFeedback(clipcounter%2);
        }    
    }

    private void Update()
    {
        if (Vector3.Distance(StartingPosition, transform.position) > stepDistance)
        {
            PlayFootstepSound();
            timer = 0;
            isStandingStill = false;
            StartingPosition = transform.position;
        }
        if (PrevPosition == transform.position && !isStandingStill)
        {
            timer += Time.deltaTime;
            if (timer >= StandingStillThreshold)
            {
                timer = 0;
                clipcounter = 0;
                StartingPosition = transform.position;
                isStandingStill = true;
            }
        }
        PrevPosition = transform.position;
    }
}
