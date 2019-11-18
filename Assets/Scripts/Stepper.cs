using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stepper : MonoBehaviour
{

    [SerializeField] Transform target;                          // Transform that is going to raytrace downwoards to the position the foot need to be at
    [SerializeField] ProceduralAnimationsController mainBody;   // Get the velocity of the main body from here
    [SerializeField] float forwardStepAtDistance;               // Distance after which the leg is going to move going forwards
    [SerializeField] float forwardOverShootFraction;            // Fraction to overstep by so the legs do not get dragged behind the body
    [SerializeField] float forwardMultiplier;                   // Multiplier to speed up the leg movement going forwards
    [SerializeField] float backwardStepAtDistance;             // Distance after which the leg is going to move when going backwards
    [SerializeField] float backwardOverShootFraction;           // Fraction to overstep by so the legs do not get dragged behind the body when going backwards
    [SerializeField] float backwardMultiplier;                  // Multiplier to speed up the leg movement going backwards
    public bool Moving;                                         // Checks if the leg is already moving
    private bool isGrounded;                                    // Boolean to check if the feet is in the air or not 


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(target.position, Vector3.down, out hit, 50.0f))
        {
            Debug.DrawRay(target.position, Vector3.down * hit.distance, Color.red);
        }
        if (Physics.Raycast(transform.position, Vector3.up, out hit, 10.0f))
        {
            Debug.DrawRay(transform.position, Vector3.up * 50.0f, Color.blue);
        }
    }

    // This function is called in the procedural animations controller
    public void TryMove()
    {
        // If already moving not start another move
        if (Moving) return;

        // Start courotine when distance to target is greater or equal than the step distance or if the feet is in the air or if it is inside another object
        RaycastHit hitDown;
        Physics.Raycast(target.position, Vector3.down, out hitDown, 50.0f);
        RaycastHit hitUp;
        if(!mainBody.movingForward && (Vector3.Distance(transform.position, hitDown.point) >= backwardStepAtDistance || !isGrounded))
        {
            StartCoroutine(MoveLeg(backwardStepAtDistance, backwardOverShootFraction, backwardMultiplier));
        }
        else if (Vector3.Distance(transform.position, hitDown.point) >= forwardStepAtDistance || !isGrounded || Physics.Raycast(transform.position, Vector3.up, out hitUp, 2.0f))
        {
            StartCoroutine(MoveLeg(forwardStepAtDistance, forwardOverShootFraction, forwardMultiplier));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8 && !isGrounded)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.layer == 8 && isGrounded )
        {
            isGrounded = false;
        }
    }

    // Coroutine that is going to take care of the leg movement
    IEnumerator MoveLeg(float stepDistance, float overShootFraction, float multiplier)
    {
        Moving = true;
        // Starting position and rotation
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        //Final position and rotation
        Vector3 endPos = new Vector3();
        Quaternion endRot = target.transform.rotation;
        // The end positions is calculated by casting a ray downwards from the target 
        RaycastHit hit;
        if(Physics.Raycast(target.position,Vector3.down, out hit, 50.0f))
        {
             Debug.DrawRay(target.position, Vector3.down * hit.distance, Color.red);
            endPos = hit.point;
        }
        // Total distance to overshoot by
        float overshootDistance = stepDistance * overShootFraction;
        Vector3 overshootVector = (endPos - transform.position) * overshootDistance;
        // Restrict the overshoot vector to the xz plane
        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);
        // Final end position
        endPos += overshootVector;
        // Get middle position between initial and end positions
        Vector3 midPos = (startPos + endPos) / 2.0f;
        // Lift mid pos
        midPos += Vector3.up * Vector3.Distance(startPos, endPos);

        // Distance from start position to end position
        float journeyLength = Vector3.Distance(startPos, endPos);
        float journeyLengthPart1 = Vector3.Distance(startPos, midPos);
        float journeyLengthPart2 = Vector3.Distance(midPos, endPos);
        // Time since the step started
        float timeElapsed = 0.0f;
        // The distance already covered by the leg
        float distanceCovered = 0.0f;
        // Leg movement 
        do
        {
            timeElapsed += Time.deltaTime;
            // Calculate what distance still needs to be covered
            distanceCovered = timeElapsed * (mainBody.moveSpeed) * stepDistance * multiplier;
            float fractionOfJourney = distanceCovered / journeyLength;
            fractionOfJourney = Easing.Cubic.InOut(fractionOfJourney);
            float fractionOfJouneyPart1 = distanceCovered / journeyLengthPart1;
            fractionOfJouneyPart1 = Easing.Cubic.InOut(fractionOfJouneyPart1);
            float fractionOfJourneyPart2 = distanceCovered / journeyLengthPart2;
            fractionOfJourneyPart2 = Easing.Cubic.InOut(fractionOfJourneyPart2);
            // Interpolate position and rotation
            transform.position = Vector3.Lerp(Vector3.Lerp(startPos, midPos, fractionOfJouneyPart1), Vector3.Lerp(midPos, endPos, fractionOfJourneyPart2), fractionOfJourney);
            transform.rotation = Quaternion.Slerp(startRot, endRot, fractionOfJourney); 
            // Wait one frame
            yield return null;
        } while (distanceCovered < journeyLength);

        // Movement finished
        Moving = false;
    }
}
