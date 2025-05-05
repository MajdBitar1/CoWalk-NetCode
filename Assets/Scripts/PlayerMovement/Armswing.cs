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

    private Vector3 prevPosLeft, prevPosRight, prevHipDirection;
    private Vector3 HipDirection,HeadDirection;
    private Vector3 PlayerCurrentPosition, PlayerPreviousFramePosition;
    public PlayerMovementData _playermovementdata;

    [Header("Track These")]
    public float playerspeed;
    public float playerprevspeed;
    public float LeftDistanceMoved = 0;
    public float RightDistanceMoved = 0;

    [Header("Access These by Other Classes")]
    private float FinalPlayerSpeed = 0;


    [Header("Tuning Parameters")]
    //[SerializeField] float SpeedAmplifier = 100f;
    [SerializeField] float friction = 1f;
    [SerializeField] float MinimumPlayerSpeedThreshold = 0.2f;
    [SerializeField] float MaximumPlayerSpeedThreshold = 100f;
    [SerializeField] float DifferenceHeadandDirection = 0.6f;
    //[SerializeField] float RotationDetectionThreshold = 0.98f;

    [Header("Smoothing Movement")]
    public float MovementAmplifier = 1;
    [SerializeField] int BufferWindow = 30;
    private Queue<float> SpeedStack = new Queue<float>();

    /// <summary>
    /// Gets a descendant GameObject with a specific name
    /// </summary>
    /// <param name="go">The parent object that is searched for a named child.</param>
    /// <param name="name">Name of child to be found.</param>
    /// <returns>The returned child GameObject or null if no child is found.</returns>
    private GameObject GetNamedChild(GameObject go, string name)
    {
        List<Transform> k_Transforms = new List<Transform>();
        k_Transforms.Clear();
        go.GetComponentsInChildren(k_Transforms);
        var foundObject = k_Transforms.Find(currentTransform => currentTransform.name == name);
        k_Transforms.Clear();

        if (foundObject != null)
            return foundObject.gameObject;

        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        _Hips = null;
        InitializeSwinger();

        //Initialize the stack with zeroes
        for (int i = 0; i < BufferWindow; i++)
        {
            SpeedStack.Enqueue(0);
        }
    }

    void Update()
    {
        updateplayermovement();
    }


    private void updateplayermovement()
    {
        FinalPlayerSpeed = SmoothMovement(ComputeMovement());
        //Debug.Log($"Final Player Speed: {FinalPlayerSpeed}");
    }


    public void InitializeSwinger()
    {
        HeadDirection = _Head.gameObject.transform.forward.normalized;
        PlayerPreviousFramePosition = transform.localPosition;
        prevPosLeft = _Lefthand.transform.localPosition;
        prevPosRight = _RightHand.transform.localPosition;
        prevHipDirection = HipDirection;
        playerspeed = 0;
        playerprevspeed = 0;
    }

    private void SetupHips()
    {
        if (GameManager.LocalPlayerObject != null)
        {
            GameObject hipsobj = GetNamedChild(GameManager.LocalPlayerObject, "Joint Chest"); //"RTRig_SpineStart"
            if (hipsobj != null)
            {
                _Hips = hipsobj;
            }
            else
            {
                Debug.LogError("[ArmSwing] Child 'RTRig_SpineStart' not found in LocalPlayerObject.");
            }
        }
        else
        {
            Debug.LogError("[ArmSwing] LocalPlayerObject not found.");
        }
    }


    private float ComputeMovement()
    {
        if (_Hips == null)
        {
            SetupHips();
            _playermovementdata = new PlayerMovementData(transform.position, HipDirection, 0, 1);
            return 0;
        }
        //Update Position
        PlayerCurrentPosition = transform.localPosition;
        //Define Direction
        HeadDirection = _Head.gameObject.transform.forward.normalized;
        HipDirection = _Hips.gameObject.transform.forward.normalized * -1;

        Vector3 NormalVec = Vector3.Cross(HipDirection, Vector3.up).normalized;

        LeftDistanceMoved = ComputeLeftHandMovement(NormalVec);
        RightDistanceMoved = ComputeRightHandMovement(NormalVec);

        float playerDistanceMoved = Vector3.Distance(PlayerCurrentPosition, PlayerPreviousFramePosition);

        float totalmovement = (LeftDistanceMoved + RightDistanceMoved)*MovementAmplifier - 2 * playerDistanceMoved;

        playerspeed = playerprevspeed * (0.9f - friction * playerprevspeed) + totalmovement;
        float misalignment = Vector3.Dot(HipDirection, HeadDirection);

        if (misalignment < DifferenceHeadandDirection)
        {
            Debug.Log("[ArmSwing] Head and Hip MisAlignment");
            //playerspeed = playerspeed * misalignment;
        }

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
        if (SpeedStack.Count >= BufferWindow)
        {
            SpeedStack.Dequeue();
        }
        SpeedStack.Enqueue(currentspeed);
        float averagespeed = SpeedStack.Sum() / BufferWindow;
        return averagespeed;
    }
    public PlayerMovementData GetSpeedFromSwings()
    {   
        return new PlayerMovementData(transform.position, HipDirection, FinalPlayerSpeed, 1);
    }

    //TO DO
    //MAKE THEM ONE FUCITON WITH LEFTHAND OR RIGHTHAND AS INPUT, 
    //BUT THIS MAY CAUSE SOME PROBLEMS WITH VARIABLES, U CAN PASS A BOOL TO SPECIFY WHICH VARIALBES TO ACCESS IN AN 2D ARRAY.
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

