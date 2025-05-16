using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(PlayerMovementHandler))]
public class Armswing : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    [Header("Player Obj Ref")]
    [SerializeField] GameObject _Lefthand;
    [SerializeField] GameObject _RightHand;
    [SerializeField] GameObject _Head;
    [SerializeField] GameObject _Hips;

    [Header("Tuning Parameters")]
    [SerializeField] float MovementAmplifier = 1;
    [SerializeField] float friction = 1f;
    [SerializeField] float MinimumPlayerSpeedThreshold = 0.2f;
    [SerializeField] float MaximumPlayerSpeedThreshold = 100f;

    //[SerializeField] float DifferenceHeadandDirection = 0.6f;
    //[SerializeField] float RotationDetectionThreshold = 0.98f;

    [Header("Smoothing Movement")]
    [SerializeField] int BufferWindow = 30;

    Vector3 prevPosLeft, prevPosRight, prevHipDirection;
    Vector3 HipDirection,HeadDirection;
    Vector3 PlayerCurrentPosition, PlayerPreviousFramePosition;

    float playerspeed;
    float playerprevspeed;
    float LeftDistanceMoved = 0;
    float RightDistanceMoved = 0;
    float FinalPlayerSpeed = 0;

    Queue<float> SpeedQueue = new Queue<float>();

    // Start is called before the first frame update
    void Start()
    {
        //_Hips = null;
        InitializeSwinger();

        //Initialize the stack with zeroes
        for (int i = 0; i < BufferWindow; i++)
        {
            SpeedQueue.Enqueue(0);
        }
    }

    void Update()
    {
        updateplayermovement();
    }

    public PlayerMovementData GetSpeedFromSwings()
    {
        return new PlayerMovementData(transform.position, HipDirection, FinalPlayerSpeed, 1);
    }


    private void updateplayermovement()
    {
        FinalPlayerSpeed = SmoothMovement(ComputeMovement());
    }


    private void InitializeSwinger()
    {
        HeadDirection = _Head.gameObject.transform.forward.normalized;
        PlayerPreviousFramePosition = transform.localPosition;
        prevPosLeft = _Lefthand.transform.localPosition;
        prevPosRight = _RightHand.transform.localPosition;
        prevHipDirection = HipDirection;
        playerspeed = 0;
        playerprevspeed = 0;
    }


    private float ComputeMovement()
    {
        if (_Hips == null)
        {
            return 0;
        }
        //Update Position
        PlayerCurrentPosition = transform.localPosition;
        //Define Direction
        HeadDirection = _Head.gameObject.transform.forward.normalized;
        HipDirection = _Hips.gameObject.transform.forward.normalized;

        Vector3 NormalVec = Vector3.Cross(HipDirection, Vector3.up).normalized;

        LeftDistanceMoved = ComputeLeftHandMovement(NormalVec);
        RightDistanceMoved = ComputeRightHandMovement(NormalVec);

        float playerDistanceMoved = Vector3.Distance(PlayerCurrentPosition, PlayerPreviousFramePosition);

        float totalmovement = (LeftDistanceMoved + RightDistanceMoved)*MovementAmplifier - 2 * playerDistanceMoved;

        playerspeed = playerprevspeed * (0.9f - friction * playerprevspeed) + totalmovement;
        float misalignment = Vector3.Dot(HipDirection, HeadDirection);

        //if (misalignment < DifferenceHeadandDirection)
        //{
        //    Debug.Log("[ArmSwing] Head and Hip MisAlignment");
        //    //playerspeed = playerspeed * misalignment;
        //}

        //Reduce Speed For these conditions
        if (playerspeed < MinimumPlayerSpeedThreshold)
        {
            playerspeed = 0;
        }

        if (playerspeed > MaximumPlayerSpeedThreshold)
        {
            playerspeed = MaximumPlayerSpeedThreshold;
        }


        //Setup parameters for next frame
        prevPosLeft = _Lefthand.transform.localPosition;
        prevPosRight = _RightHand.transform.localPosition;
        prevHipDirection = HipDirection;
        PlayerPreviousFramePosition = PlayerCurrentPosition;
        playerprevspeed = playerspeed;

        //Debug.Log($"Total Speed: {playerspeed}");
        return playerspeed;
    }
    private float SmoothMovement(float currentspeed)
    {
        if (SpeedQueue.Count >= BufferWindow)
        {
            SpeedQueue.Dequeue();
        }
        SpeedQueue.Enqueue(currentspeed);
        float averagespeed = SpeedQueue.Sum() / BufferWindow;
        return averagespeed;
    }

    private float ComputeLeftHandMovement(Vector3 NormalVec)
    {
        Vector3 CurrentLeftPos = new Vector3(_Lefthand.transform.localPosition.x, 0, _Lefthand.transform.localPosition.z);
        Vector3 DeltaLeftHand = _Lefthand.transform.localPosition - prevPosLeft;

        float LeftDistanceMoved = Mathf.Abs( Vector3.Dot(DeltaLeftHand, HipDirection) );

        if ( Vector3.Dot(NormalVec, DeltaLeftHand.normalized) >= 0.5)
        {
            LeftDistanceMoved = 0;
        }
        LeftDistanceMoved = LeftDistanceMoved / Time.deltaTime;

        return LeftDistanceMoved;
    }

    private float ComputeRightHandMovement(Vector3 NormalVec)
    {
        Vector3 CurrentRightPos = new Vector3(_RightHand.transform.localPosition.x, 0, _RightHand.transform.localPosition.z);
        Vector3 DeltaRightHand = _RightHand.transform.localPosition - prevPosRight;

        float RightDistanceMoved = Mathf.Abs( Vector3.Dot(DeltaRightHand, HipDirection));

        if (Vector3.Dot(NormalVec, DeltaRightHand.normalized) >= 0.5)
        {
            RightDistanceMoved = 0;
        }
        RightDistanceMoved = RightDistanceMoved / Time.deltaTime;

        return RightDistanceMoved;
    }
}

