using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementHandler))]
public class ComputeArmRhythm : MonoBehaviour
{
    [Header("Player Object References")]
    [SerializeField] GameObject m_lefthand;
    [SerializeField] GameObject m_righthand;

    [Header("Debugging")]
    [SerializeField] float Minimum = 0.2f;
    [SerializeField] float Maximum = 3f;
    float ResetTimer = 0;
    public float averagecycleduration;


    Vector3 leftPrevPosition, rightPrevPosition;

    [SerializeField] float prevVelocityLeft, prevVelocityRight;

    [SerializeField] float prevAccelerationLeft, prevAccelerationRight;

    public List<float> leftCycleDurations = new List<float>();
    public List<float> rightCycleDurations = new List<float>();

    public float leftTimer = 0;
    public float rightTimer = 0;

    public float leftCycleDuration = 0;
    public float rightCycleDuration = 0;

    void Start()
    {        
        leftPrevPosition = m_lefthand.transform.localPosition;
        rightPrevPosition = transform.position;

        prevVelocityLeft = 0;
        prevVelocityRight = 0;
        prevAccelerationLeft = 0;
        prevAccelerationRight = 0;
    }
    private void Update()
    {
        ComputeRhythm();
    }

    void ComputeRhythm()
    {
        leftTimer += Time.deltaTime;
        rightTimer += Time.deltaTime;

        LeftHandInflection();
        RightHandInflection();
        leftPrevPosition = m_lefthand.transform.localPosition;
        rightPrevPosition = m_righthand.transform.localPosition;
    }

    void LeftHandInflection()
    {
        float leftVelocity = Vector3.Distance(leftPrevPosition, m_lefthand.transform.localPosition);
        float leftAcceleration = leftVelocity - prevVelocityLeft;

        //At the point of inflection, the acceleration become Zero
        if (leftAcceleration * prevAccelerationLeft < 0)
        {
            //Inflection point detected
            //Check Time before the previous inflection
            if (leftTimer < Minimum || leftTimer > Maximum)
            {
                leftTimer = 0;
            }
            else
            {
                leftCycleDuration = leftTimer;
                leftCycleDurations.Add(leftCycleDuration);
                leftTimer = 0;
                Debug.Log("[RHY] Left Cycle Duration: " + leftCycleDuration);

            }

        }

    }

    void RightHandInflection()
    {
        float rightVelocity = Vector3.Distance(rightPrevPosition, m_righthand.transform.localPosition);
        float rightAcceleration = rightVelocity - prevVelocityRight;
        //At the point of inflection, the acceleration is become Zero
        if (rightAcceleration * prevAccelerationRight < 0)
        {
            //Inflection point detected
            //Check Time before the previous inflection
            if (rightTimer < Minimum || rightTimer > Maximum)
            {
                rightTimer = 0;
            }
            else
            {
                rightCycleDuration = rightTimer;
                rightCycleDurations.Add(rightCycleDuration);
                rightTimer = 0;
                Debug.Log("[RHY] Right Cycle Duration: " + rightCycleDuration);
            }
        }
    }
}
