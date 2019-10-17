using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeckoController : MonoBehaviour
{

    // Target to track
    [SerializeField] Transform target;
    // Reference to the gecko's neck
    [SerializeField] Transform headBone;
    // Head damping speed
    [SerializeField] float headTrackingTargetSpeed;
    // Max angle the head can turn
    [SerializeField] float headMaxTurnAngle;

    // Eye bones
    [SerializeField] Transform leftEyeBone;
    [SerializeField] Transform rightEyeBone;
    // Eye damping speed
    [SerializeField] float eyeTrackingSpeed;
    // Max and min eyes rotations
    [SerializeField] float leftEyeMaxYRotation;
    [SerializeField] float leftEyeMinYRotation;
    [SerializeField] float rightEyeMaxYRotation;
    [SerializeField] float rightEyeMinYRotation;

    // Legs
    [SerializeField] LegStepper frontLeftLegStepper;
    [SerializeField] LegStepper frontRightLegStepper;
    [SerializeField] LegStepper backLeftLegStepper;
    [SerializeField] LegStepper backRightLegStepper;

    // Speeds for moving and turning
    [SerializeField] float turnSpeed;
    [SerializeField] float moveSpeed;
    // Accelerations
    [SerializeField] float turnAcceleration;
    [SerializeField] float moveAcceleration;
    // Range from target
    [SerializeField] float minDistToTarget;
    [SerializeField] float maxDistToTarget;
    // Max angle to target
    [SerializeField] float maxAngToTarget;

    // World space velocity
    Vector3 currentVelocity;
    // Angular velocity, only around Y axis
    float currentAngularVelocity;

    private void Awake()
    {
        StartCoroutine(LegUpdateCoroutine());
    }
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {

    }

    /* All the animation code goes in LateUpdate.
     * This allows other systems to update the environment first allowing
     * the animation system to adapt
     */
    private void LateUpdate()
    {
        RootMotionUpdate();
        HeadTrackingUpdate();
        EyeTrackingUpdate();
    }

    private void HeadTrackingUpdate()
    {
        // Store the current head rotation 
        Quaternion currentLocalRotation = headBone.localRotation;
        // Reset head rotation
        headBone.localRotation = Quaternion.identity;
        // Get vector pointing from head to the target position
        Vector3 targetWorldLookDir = target.position - headBone.position;
        Vector3 targetLocalLookDir = headBone.parent.InverseTransformDirection(targetWorldLookDir);
        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(Vector3.forward, targetLocalLookDir, Mathf.Deg2Rad * headMaxTurnAngle, 0);
        // The rotation that we want to implement to the headBone
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);
        // Smooth the rotation through damping using slerp
        headBone.localRotation = Quaternion.Slerp(currentLocalRotation, targetLocalRotation, 1 - Mathf.Exp(-headTrackingTargetSpeed * Time.deltaTime));
    }

    private void EyeTrackingUpdate()
    {
        // Rotation to impletemt in the eyes
        Quaternion targetEyeRotation = Quaternion.LookRotation(target.position - headBone.position, transform.up);
        // Left eye rotation smoothing it using slerp
        leftEyeBone.rotation = Quaternion.Slerp(leftEyeBone.rotation, targetEyeRotation, 1 - Mathf.Exp(-eyeTrackingSpeed * Time.deltaTime));
        // Right eye rotation smoothing it using slerp
        rightEyeBone.rotation = Quaternion.Slerp(rightEyeBone.rotation, targetEyeRotation, 1 - Mathf.Exp(-eyeTrackingSpeed * Time.deltaTime));
        // Current rotation of the eyes in the y axis
        float leftEyeCurrentYRotation = leftEyeBone.localEulerAngles.y;
        float rightEyeCurrentYRotation = rightEyeBone.localEulerAngles.y;
        // Move the rotation to a -180 ~ 180 range
        if (leftEyeCurrentYRotation > 180)
        {
            leftEyeCurrentYRotation -= 360;
        }
        if (rightEyeCurrentYRotation > 180)
        {
            rightEyeCurrentYRotation -= 360;
        }
        // Clamp the Y axis rotation
        float leftEyeClampedYRotation = Mathf.Clamp(leftEyeCurrentYRotation, leftEyeMinYRotation, leftEyeMaxYRotation);
        float rightEyeClampedYRotation = Mathf.Clamp(rightEyeCurrentYRotation, rightEyeMinYRotation, rightEyeMaxYRotation);
        // Apply the clamped Y rotation without changing the X and Z rotations
        leftEyeBone.localEulerAngles = new Vector3(leftEyeBone.localEulerAngles.x, leftEyeClampedYRotation, leftEyeBone.localEulerAngles.z);
        rightEyeBone.localEulerAngles = new Vector3(rightEyeBone.localEulerAngles.x, rightEyeClampedYRotation, rightEyeBone.localEulerAngles.z);
    }

    // Only diagonal pairs of legs can move at the same time
    IEnumerator LegUpdateCoroutine()
    {
        // Run forever
        while (true)
        {
            // Try moving one diagonal pair of legs
            do
            {
                frontLeftLegStepper.TryMove();
                backRightLegStepper.TryMove();
                // Wait a frame 
                yield return null;
                // Stay in this loop while any of the legs try to move
            } while (frontLeftLegStepper.Moving || backRightLegStepper.Moving);

            // Do the same as before for the other pair of legs
            do
            {
                frontRightLegStepper.TryMove();
                backLeftLegStepper.TryMove();
                // Wait a frame
                yield return null;
            } while (frontRightLegStepper.Moving || backLeftLegStepper.Moving);
        }
    }

    // Gecko movement
    void RootMotionUpdate()
    {
        // Get direction towards target
        Vector3 targetDir = target.position - transform.position;
        // Vector toward tarzed on the XZ plane
        Vector3 targetDirProjected = Vector3.ProjectOnPlane(targetDir, transform.up);
        // Angle from gecko forward direction toward our target
        float angToTarget = Vector3.SignedAngle(transform.forward, targetDirProjected, transform.up);

        float targetAngularVelocity = 0.0f;

        // If within max angle leave angular velocity to zero
        if (Mathf.Abs(angToTarget) > maxAngToTarget)
        {
            // Positive angle means right
            if (angToTarget > 0)
            {
                targetAngularVelocity = turnSpeed;
            }
            else
            {
                targetAngularVelocity = -turnSpeed;
            }
        }

        // Use smooth function to gradually change the velocity
        currentAngularVelocity = Mathf.Lerp(currentAngularVelocity, targetAngularVelocity, 1 - Mathf.Exp(-turnAcceleration * Time.deltaTime));
        // Rotate transform around the y axis
        transform.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);

        Vector3 targetVelocity = Vector3.zero;
        // Dont move if gecko is not facing the target
        if (Mathf.Abs(angToTarget) < 90)
        {
            float distToTarget = Vector3.Distance(transform.position, target.position);

            // If gecko is too far away, approach the target
            if (distToTarget > maxDistToTarget)
            {
                targetVelocity = moveSpeed * targetDirProjected.normalized;
            }
            // If gecko is too close reverse direction and move away
            else if (distToTarget < minDistToTarget)
            {
                targetVelocity = moveSpeed * -targetDirProjected.normalized;
            }
        }

        // Smooth velocity
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1 - Mathf.Exp(-moveAcceleration * Time.deltaTime));

        // Apply velocity
        transform.position += currentVelocity * Time.deltaTime;
    }
}
