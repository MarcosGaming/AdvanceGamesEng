using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralStabilizer : MonoBehaviour
{
    [SerializeField] Transform mainBody;                            // Body which height needs to change
    [SerializeField] float standardHeight;                          // Standard heihgt of the central body (the one at the beginning)
    [SerializeField] float maxHeightDifference;                     // Maximum height difference from the standard height permited
    [SerializeField] float changeHeightInTime;                      // The time step that is going to take for the body to change its height
    private float currentHeight;                                    // Current height of the body
    private bool changingHeight;                                    // Boolean to check if the body is already changing its height
    private bool stairs;                                            // Boolean to check if the body is going through some stairs

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Being in the stairs will affect the main body
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50.0f))
        {
            Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.red);
            if(hit.transform.tag == "Stairs")
            {
                stairs = true;
            }
            else
            {
                stairs = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (changingHeight) return;
        // Cast a raycast downwards to get the current height of the character
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50.0f))
        {
            currentHeight = Vector3.Distance(hit.point, transform.position);
        }
        // Change body height if the current height is either too low or too big
        if (currentHeight > (standardHeight + maxHeightDifference) || currentHeight < (standardHeight - maxHeightDifference))
        { 
            StartCoroutine(CorrectBodyHeight());
        }
    }

    IEnumerator CorrectBodyHeight()
    {
        changingHeight = true;
        // Time elapsed since the change started
        float timeElapsed = 0.0f;
        // The amount of height that needs to change
        float amountToChange = (Mathf.Abs(standardHeight) - Mathf.Abs(currentHeight));
        // Final main body height
        float endHeight = mainBody.position.y + amountToChange;
        do
        {
            timeElapsed += Time.deltaTime;
            // At each iteration the x and z might change but not the y
            Vector3 startPos = mainBody.position;
            Vector3 endPos = new Vector3(mainBody.position.x, endHeight, mainBody.position.z);
            mainBody.position = Vector3.Lerp(startPos, endPos, timeElapsed);
            yield return null;
        } while (timeElapsed < changeHeightInTime);

        changingHeight = false;
    }

    public bool isOnStairs()
    {
        return stairs;
    }
}
