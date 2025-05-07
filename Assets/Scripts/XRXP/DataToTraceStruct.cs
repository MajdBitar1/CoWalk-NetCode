using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DataToTrace
{
    public bool AuraState;
    public bool Footstate;
    public float SeparationDistance;
    public float SeparationAngle;

    public float ExperimenterSpeed;
    public float ParticipantSpeed;
    public float ExperimenterFreq;
    public float ParticipantFreq;
    public Vector3 ExperimenterPosition;
    public Vector3 ParticipantPosition;

    public void SetStateData(bool aurastate, bool footstate)
    {
        AuraState = aurastate;
        Footstate = footstate;
    }
    public void SetIndividualData(
        float experimenterspeed,
        float experimenterfreq,
        float participantspeed,
        float participantfreq
        )
    {
        ExperimenterSpeed = experimenterspeed;
        ParticipantSpeed = participantspeed;
        ExperimenterFreq = experimenterfreq;
        ParticipantFreq = participantfreq;
    }

    public void SetPositionData(
        Vector3 experimenterposition,
        Vector3 participantposition,
        Vector3 ExperimenterDirection,
        Vector3 ParticipantDirection
        )
    {
        ExperimenterPosition = experimenterposition;
        ParticipantPosition = participantposition;
        SeparationDistance = Vector3.Distance(experimenterposition, participantposition);
        SeparationAngle = Vector3.Angle(ExperimenterDirection, ParticipantDirection);
    }
}
