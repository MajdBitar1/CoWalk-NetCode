using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Armswing : MonoBehaviour
{
    #region Constants
    private const float FRICTION_MULTIPLIER = 0.9f;
    private const float FRICTION_SPEED_FACTOR = 2f;
    private const float MOVEMENT_AMPLIFIER_FACTOR = 2f;
    private const float SIDE_MOVEMENT_THRESHOLD = 0.5f;
    #endregion

    #region Inspector Fields
    [Header("Player Object References")]
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject head;
    [SerializeField] private GameObject hips;

    [Header("Movement Tuning")]
    [SerializeField] private float movementAmplifier = 150f;
    [SerializeField] private float friction = 1f;
    [SerializeField] private float minimumSpeedThreshold = 150f;
    [SerializeField] private float maximumSpeedThreshold = 500f;

    [Header("Smoothing")]
    [SerializeField] private int bufferWindowSize = 60;
    #endregion

    #region Private Fields
    // Position tracking
    private Vector3 previousLeftHandPosition;
    private Vector3 previousRightHandPosition;
    private Vector3 previousPlayerPosition;
    private Vector3 currentPlayerPosition;

    // Direction vectors
    private Vector3 hipDirection;
    private Vector3 headDirection;

    // Speed calculations
    private float currentSpeed;
    private float previousSpeed;
    private float leftHandDistance;
    private float rightHandDistance;
    private float finalPlayerSpeed;

    // Smoothing
    private Queue<float> speedBuffer = new Queue<float>();
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeArmswingSystem();
        InitializeSpeedBuffer();
    }

    private void Update()
    {
        finalPlayerSpeed = CalculateSmoothedMovement();
    }
    #endregion

    #region Public Interface
    /// <summary>
    /// Gets the current player movement data including position, direction, and speed
    /// </summary>
    public PlayerMovementData GetSpeedFromSwings()
    {
        return new PlayerMovementData(transform.position, hipDirection, finalPlayerSpeed, 1);
    }
    #endregion

    #region Initialization
    private void InitializeArmswingSystem()
    {
        if (!ValidateReferences()) return;

        headDirection = head.transform.forward.normalized;
        previousPlayerPosition = transform.localPosition;
        previousLeftHandPosition = leftHand.transform.localPosition;
        previousRightHandPosition = rightHand.transform.localPosition;
        
        currentSpeed = 0f;
        previousSpeed = 0f;
    }

    private void InitializeSpeedBuffer()
    {
        speedBuffer.Clear();
        for (int i = 0; i < bufferWindowSize; i++)
        {
            speedBuffer.Enqueue(0f);
        }
    }

    private bool ValidateReferences()
    {
        if (leftHand == null || rightHand == null || head == null || hips == null)
        {
            Debug.LogError("Missing required GameObject references in Armswing component!");
            return false;
        }
        return true;
    }
    #endregion

    #region Movement Calculation
    private float CalculateSmoothedMovement()
    {
        float rawMovement = CalculateRawMovement();
        return ApplySmoothing(rawMovement);
    }

    private float CalculateRawMovement()
    {
        if (hips == null) return 0f;

        UpdatePositionsAndDirections();
        
        Vector3 sideVector = CalculateSideVector();
        leftHandDistance = CalculateHandMovement(leftHand, previousLeftHandPosition, sideVector);
        rightHandDistance = CalculateHandMovement(rightHand, previousRightHandPosition, sideVector);

        float playerMovementDistance = Vector3.Distance(currentPlayerPosition, previousPlayerPosition);
        float totalHandMovement = (leftHandDistance + rightHandDistance) * movementAmplifier;
        float adjustedMovement = totalHandMovement - (MOVEMENT_AMPLIFIER_FACTOR * playerMovementDistance);

        currentSpeed = CalculateSpeedWithFriction(adjustedMovement);
        currentSpeed = ApplySpeedLimits(currentSpeed);

        UpdatePreviousFrameData();

        return currentSpeed;
    }

    private void UpdatePositionsAndDirections()
    {
        currentPlayerPosition = transform.localPosition;
        headDirection = head.transform.forward.normalized;
        hipDirection = hips.transform.forward.normalized;
    }

    private Vector3 CalculateSideVector()
    {
        return Vector3.Cross(hipDirection, Vector3.up).normalized;
    }

    private float CalculateSpeedWithFriction(float movement)
    {
        return previousSpeed * (FRICTION_MULTIPLIER - friction * previousSpeed) + movement;
    }

    private float ApplySpeedLimits(float speed)
    {
        if (speed < minimumSpeedThreshold)
            return 0f;
        
        return Mathf.Min(speed, maximumSpeedThreshold);
    }

    private void UpdatePreviousFrameData()
    {
        previousLeftHandPosition = leftHand.transform.localPosition;
        previousRightHandPosition = rightHand.transform.localPosition;
        previousPlayerPosition = currentPlayerPosition;
        previousSpeed = currentSpeed;
    }
    #endregion

    #region Hand Movement Calculation
    private float CalculateHandMovement(GameObject hand, Vector3 previousPosition, Vector3 sideVector)
    {
        Vector3 handDelta = hand.transform.localPosition - previousPosition;
        float forwardMovement = Mathf.Abs(Vector3.Dot(handDelta, hipDirection));

        // Ignore sideways movement
        if (IsSidewaysMovement(handDelta, sideVector))
        {
            return 0f;
        }

        return forwardMovement / Time.deltaTime;
    }

    private bool IsSidewaysMovement(Vector3 handDelta, Vector3 sideVector)
    {
        return Vector3.Dot(sideVector, handDelta.normalized) >= SIDE_MOVEMENT_THRESHOLD;
    }
    #endregion

    #region Smoothing
    private float ApplySmoothing(float currentSpeed)
    {
        if (speedBuffer.Count >= bufferWindowSize)
        {
            speedBuffer.Dequeue();
        }
        
        speedBuffer.Enqueue(currentSpeed);
        return speedBuffer.Average();
    }
    #endregion
}

