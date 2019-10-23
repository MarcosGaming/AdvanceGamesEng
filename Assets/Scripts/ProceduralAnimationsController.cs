using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimationsController : MonoBehaviour
{
    [SerializeField] Transform target;          // Target that the character is going to follow
    [SerializeField] Stepper[] steppers;   // Legs
    [SerializeField] float turnSpeed;           // Turn speed
    public float moveSpeed;                     // Movement speed
    [SerializeField] float turnAcceleration;    // Turn acceleration
    [SerializeField] float moveAcceleration;    // Movement acceleration
    [SerializeField] float minDistToTarget;     // Min range to target
    [SerializeField] float maxDistToTarget;     // Max range to target
    [SerializeField] float maxAngToTarget;      // Max angle to target
    Vector3 currentVelocity;                    // Current velocity
    float currentAngularVelocity;               // Current angular velocity

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
                yield return new WaitForSeconds(5);
            }
        }
    }

    // Movement
    void RootMotionUpdate()
    {
        // Get direction towards target
        Vector3 targetDir = target.position - transform.position;
        // Vector toward target on the XZ plane
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
