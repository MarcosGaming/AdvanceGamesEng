using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stepper : MonoBehaviour
{

    [SerializeField] Transform target;                          // Transform that is going to raytrace downwoards to the position the foot need to be at
    [SerializeField] ProceduralAnimationsController mainBody;   // Get the velocity of the main body from here
    [SerializeField] float stepAtDistance;                      // Distance after which the leg is going to move
    [SerializeField] float stepTime;                            // Time that will take the leg to reach its position
    public bool Moving;                                         // Checks if the leg is already moving


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
    }

    private void LateUpdate()
    {
        TryMove();
    }

    // This function is called in the movement controller
    public void TryMove()
    {
        // If already moving not start another move
        if (Moving) return;

        // Start courotine when distance to target is greater or equal than the step distance
        if (Vector3.Distance(transform.position, target.position) >= stepAtDistance)
        {
            StartCoroutine(MoveLeg());
        }
    }

    // Coroutine that is going to take care of the leg movement
    IEnumerator MoveLeg()
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
            distanceCovered = timeElapsed * (mainBody.moveSpeed) * stepAtDistance;
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
