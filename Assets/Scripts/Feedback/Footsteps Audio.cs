using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartingPosition = transform.position;
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
