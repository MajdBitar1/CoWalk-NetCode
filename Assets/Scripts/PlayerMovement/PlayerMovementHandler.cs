using System.Collections;
using UnityEngine;
using Oculus.Avatar2;

[RequireComponent(typeof(Armswing))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovementHandler : MonoBehaviour
{
    #region Constants
    private const float ANIMATION_MIN_VALUE = -0.02f;
    private const float ANIMATION_MAX_VALUE = 0.02f;
    #endregion

    #region Inspector Fields
    [Header("Animation Limits")]
    [SerializeField] private float animationMinValue = ANIMATION_MIN_VALUE;
    [SerializeField] private float animationMaxValue = ANIMATION_MAX_VALUE;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    #endregion

    #region Private Fields
    // Movement data
    private PlayerMovementData movementData;
    
    // Component references
    private Armswing armSwing;
    private CharacterController characterController;
    private MecanimLegsAnimationController legsAnimationController;
    private PlayerNetworkInfo playerNetworkInfo;
    
    // Initialization flags
    private bool isInitialized = false;
    #endregion

    #region Properties
    public PlayerMovementData MovementData => movementData;
    public bool IsMovementEnabled { get; private set; }
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeComponents();
        InitializeMovementData();
    }

    private void Update()
    {
        if (!TryCompleteInitialization()) return;

        UpdateMovementData();
        HandlePlayerMovement();
        UpdateNetworkInformation();
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        armSwing = GetComponent<Armswing>();
        characterController = GetComponent<CharacterController>();
        
        if (armSwing == null)
        {
            LogError("Armswing component not found!");
        }
        
        if (characterController == null)
        {
            LogError("CharacterController component not found!");
        }
    }

    private void InitializeMovementData()
    {
        movementData = new PlayerMovementData(transform.position, Vector3.zero, 0, 0);
    }

    private bool TryCompleteInitialization()
    {
        if (isInitialized) return true;

        // Try to get legs animation controller
        if (legsAnimationController == null)
        {
            legsAnimationController = GameManager.LocalPlayerObject?.GetComponentInChildren<MecanimLegsAnimationController>();
            if (legsAnimationController == null) return false;
        }

        // Try to get player network info
        if (playerNetworkInfo == null)
        {
            playerNetworkInfo = GameManager.LocalPlayerObject?.GetComponent<PlayerNetworkInfo>();
            if (playerNetworkInfo == null) return false;
        }

        isInitialized = true;
        LogDebug("PlayerMovementHandler initialization complete");
        return true;
    }
    #endregion

    #region Movement Processing
    private void UpdateMovementData()
    {
        if (armSwing == null) return;

        movementData = armSwing.GetSpeedFromSwings();
    }

    private void HandlePlayerMovement()
    {
        IsMovementEnabled = CheckPlayerInput();

        if (IsMovementEnabled)
        {
            MovePlayer();
        }
        else
        {
            StopPlayerMovement();
        }
    }

    private bool CheckPlayerInput()
    {
        return OVRInput.Get(OVRInput.Touch.PrimaryThumbRest) ||
               OVRInput.Get(OVRInput.Touch.SecondaryThumbRest) ||
               OVRInput.Get(OVRInput.Touch.One) ||
               OVRInput.Get(OVRInput.Touch.Two) ||
               OVRInput.Get(OVRInput.Touch.Three) ||
               OVRInput.Get(OVRInput.Touch.Four);
    }

    private void MovePlayer()
    {
        if (characterController == null) return;

        // Calculate and apply movement
        Vector3 movementVector = CalculateMovementVector();
        characterController.SimpleMove(movementVector);

        // Update leg animations
        UpdateLegAnimations(movementVector);

        LogDebug($"Player moved: {movementVector}, Speed: {movementData.Speed}");
    }

    private Vector3 CalculateMovementVector()
    {
        return movementData.Speed * movementData.Direction * Time.deltaTime;
    }

    private void UpdateLegAnimations(Vector3 movementVector)
    {
        if (legsAnimationController == null) return;

        // Map movement to animation range
        Vector3 animationValue = new Vector3(
            Mathf.Lerp(animationMinValue, animationMaxValue, NormalizeMovementComponent(movementVector.x)),
            0f,
            Mathf.Lerp(animationMinValue, animationMaxValue, NormalizeMovementComponent(movementVector.z))
        );

        legsAnimationController.armswing = animationValue;
    }

    private float NormalizeMovementComponent(float component)
    {
        // Normalize the movement component to 0-1 range for lerping
        // This assumes movement values are typically between -1 and 1
        return Mathf.Clamp01((component + 1f) * 0.5f);
    }

    private void StopPlayerMovement()
    {
        if (legsAnimationController != null)
        {
            legsAnimationController.armswing = Vector3.zero;
        }
    }
    #endregion

    #region Network Updates
    private void UpdateNetworkInformation()
    {
        if (playerNetworkInfo == null) return;

        playerNetworkInfo.UpdateValues(
            movementData.Direction,
            movementData.Speed,
            movementData.CycleDuration
        );
    }
    #endregion

    #region Public Interface
    /// <summary>
    /// Sets the movement data for the player
    /// </summary>
    /// <param name="data">The movement data to set</param>
    public void SetMovementData(PlayerMovementData data)
    {
        movementData = data;
    }

    /// <summary>
    /// Gets the current movement data
    /// </summary>
    /// <returns>Current player movement data</returns>
    public PlayerMovementData GetMovementData()
    {
        return movementData;
    }

    /// <summary>
    /// Forces a stop of all player movement
    /// </summary>
    public void ForceStopMovement()
    {
        IsMovementEnabled = false;
        StopPlayerMovement();
    }
    #endregion

    #region Validation
    private void OnValidate()
    {
        // Ensure animation values are in correct order
        if (animationMinValue > animationMaxValue)
        {
            float temp = animationMinValue;
            animationMinValue = animationMaxValue;
            animationMaxValue = temp;
        }
    }
    #endregion

    #region Debug Utilities
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerMovementHandler] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[PlayerMovementHandler] {message}", this);
    }
    #endregion
}
