using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DataToTrace
{
    public int ExperimenterState;

    public float SeparationDistance;
    public float SeparationAngle;

    public float ExperimenterSpeed;
    public float ParticipantSpeed;
    public float ExperimenterFreq;
    public float ParticipantFreq;

    public Transform ExperimenterTransform;
    public Transform ParticipantTransform;

    public Vector3 ExperimenterDirection;
    public Vector3 ParticipantDirection;

    public void SetStateData(int experimenterstate)
    {
        this.ExperimenterState = experimenterstate;
    }
    public void SetIndividualData(
        float experimenterspeed,
        float experimenterfreq,
        float participantspeed,
        float participantfreq
        )
    {
        this.ExperimenterSpeed = experimenterspeed;
        this.ParticipantSpeed = participantspeed;
        this.ExperimenterFreq = experimenterfreq;
        this.ParticipantFreq = participantfreq;
    }

    public void SetPositionData(
        Transform experimenterTransform,
        Transform participantTransform,
        Vector3 ExperimenterDirection,
        Vector3 ParticipantDirection
        )
    {
        this.ExperimenterTransform = experimenterTransform;
        this.ParticipantTransform = participantTransform;
        this.ExperimenterDirection = ExperimenterDirection;
        this.ParticipantDirection = ParticipantDirection;

        this.SeparationDistance = Vector3.Distance(experimenterTransform.position, participantTransform.position);
        this.SeparationAngle = Vector3.Angle(ExperimenterDirection, ParticipantDirection);
    }
}
