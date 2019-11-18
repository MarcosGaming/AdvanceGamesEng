using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserAttack : MonoBehaviour
{
    [SerializeField] Transform upperHead;           // Transform to move the upper part of the spider robot head
    [SerializeField] Transform upperHeadEnd;        // Transform that indicates the end position of the upper head
    [SerializeField] Transform lowerHead;           // Transform to move the lower part of the spider robot head
    [SerializeField] Transform lowerHeadEnd;        // Transform that indicates the end position of the lower head
    [SerializeField] float openInTime;              // Time step that is going to take for the head to open
    [SerializeField] float closeInTime;             // TIme step that is going to take for the head to close
    public bool isAttacking;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKeyDown)
        {
            StartCoroutine(ShootLaser());
        }
    }

    // Method that is going to be called in the procedural animations controller
    public void TryToShootLaser()
    {

    }

    IEnumerator ShootLaser()
    {
        isAttacking = true;
        // Open head parts
        Vector3 upperStart = upperHead.position;
        Vector3 upperEnd = upperHeadEnd.position;
        Vector3 lowerStart = lowerHead.position;
        Vector3 lowerEnd = lowerHeadEnd.position;
        float timeElapsed = 0.0f;
        do
        {
            timeElapsed += Time.deltaTime;
            upperHead.position = Vector3.Lerp(upperStart, upperEnd, timeElapsed);
            lowerHead.position = Vector3.Lerp(lowerStart, lowerEnd, timeElapsed);
            yield return null;
        } while (timeElapsed < openInTime);
        // Shoot laser
        yield return new WaitForSeconds(1.0f);

        yield return new WaitForSeconds(1.0f);
        // Close head parts
        timeElapsed = 0.0f;
        do
        {
            timeElapsed += Time.deltaTime;
            upperHead.position = Vector3.Lerp(upperEnd, upperStart, timeElapsed);
            lowerHead.position = Vector3.Lerp(lowerEnd, lowerStart, timeElapsed);
            yield return null;
        } while (timeElapsed < closeInTime);
        isAttacking = false;
    }
}
