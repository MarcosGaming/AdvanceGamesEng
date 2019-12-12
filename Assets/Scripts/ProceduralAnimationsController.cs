using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProceduralAnimationsController : MonoBehaviour
{
    [SerializeField] Transform target;              // Target that the character is going to follow
    [SerializeField] Transform headBone;            // Neck
    [SerializeField] float headTrackingTargetSpeed; // Head tracking speed
    [SerializeField] float headMaxTurnAngle;        // Max angle the head can turn
    private Vector3 headStartDirection;             // Starting head direction
    private Quaternion headStartRotation;           // Starting head rotation

    [SerializeField] Stepper[] steppers;            // Legs
    [SerializeField] float turnSpeed;               // Turn speed
    public float moveSpeed;                         // Movement speed
    public bool movingForward;                      // Whether the character is moving forwards or backwards
    [SerializeField] float turnAcceleration;        // Turn acceleration
    [SerializeField] float moveAcceleration;        // Movement acceleration
    [SerializeField] float minDistToTarget;         // Min range to target
    [SerializeField] float maxDistToTarget;         // Max range to target
    [SerializeField] float maxAngToTarget;          // Max angle to target
    [SerializeField] float minAngToTarget;          // Min angle to target 

    [SerializeField] Orientation orientation;       // Right or Forward

    Vector3 currentVelocity;                        // Current velocity
    float currentAngularVelocity;                   // Current angular velocity
    Vector3 fromVector;                             // The vector from which the angular difference is measured

    [SerializeField] LaserAttack laser;             // Script for the laser attack

    [SerializeField] CentralStabilizer stabilizer;  // Stabilizer that changes the height of the body when necessary

    [SerializeField] float lookAheadBackwardsBy;    // The distance that the body is going to use to look ahead when going backwards

    private NavMeshPath path;                       // Path that will mark the direction to follow

    private Vector3 previousPosition;               // Position in the previous frame

    enum Orientation
    {
        Right,
        Forward
    };

    private void Awake()
    {
        headStartRotation = headBone.rotation;
        StartCoroutine(LegUpdateCoroutine());
        path = new NavMeshPath();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HeadTrackingUpdate();
        RootMotionUpdate();
    }

    private void LateUpdate()
    {
        previousPosition = transform.position;
    }

    // Coroutine for the leg movement
    IEnumerator LegUpdateCoroutine()
    {
        // Run forever
        while (true)
        {
            for(int i = 0; i < steppers.Length; i++)
            {
                do
                {
                    steppers[i].TryMove();
                    yield return null;
                } while (steppers[i].Moving);
            }
        }
    }

    // Head tracking
    private void HeadTrackingUpdate()
    {
        Vector3 lookPos = target.position - headBone.position;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        Quaternion targetRotation = headBone.rotation;
        if (Quaternion.Angle(rotation, headStartRotation * transform.rotation) <= headMaxTurnAngle || Quaternion.Angle(headBone.rotation, headStartRotation * transform.rotation) <= headMaxTurnAngle)
        {
            targetRotation = rotation;
        }
        headBone.rotation = Quaternion.Slerp(headBone.rotation, targetRotation, 1 - Mathf.Exp(-headTrackingTargetSpeed * Time.deltaTime));
    }

    // Movement
    void RootMotionUpdate()
    {
        // Calculate path towards target
        NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
        for (int i = 0; i < path.corners.Length - 1; i++)
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        // Get direction towards target
        Vector3 targetDir = new Vector3();
        if (path.corners.Length > 0)
        {
           targetDir = path.corners[1] - transform.position;
        }
        // Vector toward target on the XZ plane
        Vector3 targetDirProjected = Vector3.ProjectOnPlane(targetDir, transform.up);
        // Angle from forward direction toward our target
        if (orientation.Equals(Orientation.Forward))
        {
            fromVector = transform.forward;
        }
        else
        {
            fromVector = transform.right;
        }
        float angToTarget = Vector3.SignedAngle(fromVector, targetDirProjected, transform.up);
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
        // Dont move if not facing the target
        if (Mathf.Abs(angToTarget) < minAngToTarget)
        {
            float distToTarget = Vector3.Distance(transform.position, target.position);

            // If too far away, approach the target
            if (distToTarget > maxDistToTarget)
            {
                targetVelocity = moveSpeed * targetDirProjected.normalized;
                laser.StopShootLaser();
            }
            // If too close reverse direction and move away
            else if (distToTarget < minDistToTarget)
            {
                targetVelocity = moveSpeed * -targetDirProjected.normalized;
                laser.StopShootLaser();
            }
            // If between min and max distances, try to shoot the laser
            else
            {
                laser.TryShootLaser();
            }
        }
        else
        {
            currentVelocity *= 0.5f;
            laser.StopShootLaser();
        }
        // When going up or down stairs reduce the velocity
        if(stabilizer.isOnStairs())
        {
            targetVelocity *= 0.7f;
        }

        // Smooth velocity
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1 - Mathf.Exp(-moveAcceleration * Time.deltaTime));

        // When going backwards check that the new position is reachable
        if(!movingForward && !NavMesh.CalculatePath(transform.position, transform.position + (currentVelocity * Time.deltaTime * lookAheadBackwardsBy), NavMesh.AllAreas, path))
        {
            currentVelocity = Vector3.zero;
        }

        // Apply velocity
        transform.position += currentVelocity * Time.deltaTime;

        // Calculate whether moving forwards or backwards
        if (Vector3.Dot(fromVector, (transform.position - previousPosition)) < 0.0f)
        {
            movingForward = false;
        }
        else
        {
            movingForward = true;
        }
    }
}
