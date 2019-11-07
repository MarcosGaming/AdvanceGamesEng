﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimationsController : MonoBehaviour
{
    [SerializeField] Transform target;              // Target that the character is going to follow
    [SerializeField] Transform headBone;            // Neck
    [SerializeField] float headTrackingTargetSpeed; // Head tracking speed
    [SerializeField] float headMaxTurnAngle;        // Max angle the head can turn
    private Vector3 headStartDirection;             // Starting head direction
    private Quaternion headStartRotation;           // Starting head rotation
    [SerializeField] Stepper[] steppers;            // Legs
    [SerializeField] CentralStabilizer centralBody; // Component that manages changes in the central body
    [SerializeField] float turnSpeed;               // Turn speed
    public float moveSpeed;                         // Movement speed
    public bool movingForward;                      // Whether the character is moving forwards or backwards
    [SerializeField] float turnAcceleration;        // Turn acceleration
    [SerializeField] float moveAcceleration;        // Movement acceleration
    [SerializeField] float minDistToTarget;         // Min range to target
    [SerializeField] float maxDistToTarget;         // Max range to target
    [SerializeField] float maxAngToTarget;          // Max angle to target
    [SerializeField] Orientation orientation;       // Right or Forward
    Vector3 currentVelocity;                        // Current velocity
    float currentAngularVelocity;                   // Current angular velocity
    Vector3 fromVector;                             // The vector from which the angular difference is measured

    enum Orientation
    {
        Right,
        Forward
    };

    private void Awake()
    {
        headStartDirection = target.position - headBone.position;
        headStartRotation = headBone.rotation;
        StartCoroutine(LegUpdateCoroutine());
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
         // Store the current head rotation 
         /*Quaternion currentLocalRotation = headBone.localRotation;
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
         headBone.localRotation = Quaternion.Slerp(currentLocalRotation, targetLocalRotation, 1 - Mathf.Exp(-headTrackingTargetSpeed * Time.deltaTime))*/;

        // Limit angle by which the head can rotate
        /*Vector3 desiredHeadDir = Vector3.RotateTowards(headBone.right, target.position - headBone.position, Mathf.Deg2Rad * headMaxTurnAngle, 0);
        //desiredHeadDir.x = 0;
        // Get the final rotation
        Quaternion desiredRotation = Quaternion.FromToRotation(headStartDirection, desiredHeadDir) * headStartRotation;
        //Quaternion desiredRotation = Quaternion.LookRotation(desiredHeadDir, transform.up) * headStartRotation;
        // Smooth rotation using slerp
        headBone.rotation = Quaternion.Slerp(headBone.rotation, desiredRotation, 1 - Mathf.Exp(-headTrackingTargetSpeed * Time.deltaTime));*/


        Vector3 lookPos = target.position - headBone.position;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        Quaternion targetRotation = headBone.rotation;
        if (Quaternion.Angle(rotation, headStartRotation * transform.rotation) <= headMaxTurnAngle || Quaternion.Angle(headBone.rotation, headStartRotation * transform.rotation) <= headMaxTurnAngle)
        {
            targetRotation = rotation;
        }
        headBone.rotation = Quaternion.Slerp(headBone.rotation, targetRotation, 1 - Mathf.Exp(-headTrackingTargetSpeed * Time.deltaTime));

        /*Vector3 targetHeadDir = Vector3.RotateTowards(headBone.forward, target.position - headBone.position, Mathf.Deg2Rad * headMaxTurnAngle, 0);
        targetHeadDir.z = 0;
        Quaternion targetRotation = Quaternion.LookRotation(targetHeadDir);
        if (Quaternion.Angle(targetRotation, headStartRotation) <= headMaxTurnAngle)
        {
            headBone.rotation = Quaternion.Slerp(headBone.rotation, targetRotation, 1 - Mathf.Exp(-headTrackingTargetSpeed * Time.deltaTime));
        }*/
    }

    // Movement
    void RootMotionUpdate()
    {
        // Get direction towards target
        Vector3 targetDir = target.position - transform.position;
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
        if (Mathf.Abs(angToTarget) < 90)
        {
            float distToTarget = Vector3.Distance(transform.position, target.position);
            movingForward = true;

            // If too far away, approach the target
            if (distToTarget > maxDistToTarget)
            {
                targetVelocity = moveSpeed * targetDirProjected.normalized;
            }
            // If too close reverse direction and move away
            else if (distToTarget < minDistToTarget)
            {
                movingForward = false;
                targetVelocity = moveSpeed * -targetDirProjected.normalized;
            }
        }

        // Smooth velocity
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1 - Mathf.Exp(-moveAcceleration * Time.deltaTime));

        // Apply velocity
        transform.position += currentVelocity * Time.deltaTime;
    }
}
