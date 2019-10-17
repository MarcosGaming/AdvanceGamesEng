using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stepper : MonoBehaviour
{

    [SerializeField] Transform target;      // Transform that is going to raytrace downwoards to the position the foot need to be at
    [SerializeField] float stepAtDistance;  // Distance after which the leg is going to move
    public bool Moving;                     // Checks if the leg is already moving


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
        if (Moving) return;
        if(Vector3.Distance(transform.position, target.position) > stepAtDistance)
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
        //Final position and rotatuion
        Vector3 endPos = new Vector3();
        Quaternion endRot = target.transform.rotation;
        // The end positions is calculated by casting a ray downwards from the target (target is at the height of the knees and a bit ahead of them)
        RaycastHit hit;
        if(Physics.Raycast(target.position,Vector3.down, out hit, 50.0f))
        {
             Debug.DrawRay(target.position, Vector3.down * hit.distance, Color.red);
            endPos = hit.point;
        }

        // Distance from start position to end position
        float journeyLength = Vector3.Distance(startPos, endPos);
        // Time since the step started
        float timeElapsed = 0.0f;
        // Distance covered by the lef/foot
        float distanceCovered = 0.1f;
        // Leg movement 
        do
        {
            timeElapsed += Time.deltaTime;
            // Calculate what distance still needs to be covered
            distanceCovered = timeElapsed * 1.0f;
            float fractionOfJourney = distanceCovered / journeyLength;
            // Interpolate position and rotation
            transform.position = Vector3.Lerp(startPos, endPos, fractionOfJourney);
            transform.rotation = Quaternion.Slerp(startRot, endRot, fractionOfJourney);
            print(fractionOfJourney);
            // Wait one frame
            yield return null;
        } while (distanceCovered < journeyLength);

        // Movement finished
        Moving = false;
    }
}
