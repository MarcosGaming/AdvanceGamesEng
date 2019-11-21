using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserAttack : MonoBehaviour
{
    [SerializeField] Transform upperHead;           // Transform to move the upper part of the spider robot head
    [SerializeField] Transform upperHeadStart;      // Transform that indicates the start position of the upper head
    [SerializeField] Transform upperHeadEnd;        // Transform that indicates the end position of the upper head
    [SerializeField] Transform lowerHead;           // Transform to move the lower part of the spider robot head
    [SerializeField] Transform lowerHeadStart;      // Transform that indicates the start position of the lower head
    [SerializeField] Transform lowerHeadEnd;        // Transform that indicates the end position of the lower head
    [SerializeField] float openInTime;              // Time step that is going to take for the head to open
    [SerializeField] float closeInTime;             // TIme step that is going to take for the head to close
    [SerializeField] GameObject laserPrefab;        // Laser game object
    [SerializeField] GameObject firePoint;          // Gameobject from where the laser is going to be shot
    [SerializeField] float fireInTime;              // For how long the laser is going to be shot
    [SerializeField] float fireEverySeconds;        // Seconds after which the laser might be fired

    private GameObject spawnedLaser;                // Instance of the laser prefab
    private float countdown;                        // Countdown until the laser is fired
    private bool isAttacking;                       // Whether the laser is being shot or not

    // Start is called before the first frame update
    void Start()
    {
        spawnedLaser = Instantiate(laserPrefab, firePoint.transform) as GameObject;
        spawnedLaser.SetActive(false);
        countdown = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        countdown += Time.deltaTime;
        if(countdown >= fireEverySeconds)
        {
            // Decide randomly whether to shoot the laser or not if the laser is not already being shot
            if(Random.Range(0.0f, 1.0f) > 0.5f && !isAttacking)
            {
                StartCoroutine(ShootLaser());
            }
            countdown = 0.0f;
        }
    }

    IEnumerator ShootLaser()
    {
        isAttacking = true;
        // Open head parts
        float timeElapsed = 0.0f;
        do
        {
            timeElapsed += Time.deltaTime;
            upperHead.position = Vector3.Lerp(upperHeadStart.position, upperHeadEnd.position, timeElapsed);
            lowerHead.position = Vector3.Lerp(lowerHeadStart.position, lowerHeadEnd.position, timeElapsed);
            yield return null;
        } while (timeElapsed < openInTime);
        // Shoot laser
        timeElapsed = 0.0f;
        spawnedLaser.SetActive(true);
        do
        {
            timeElapsed += Time.deltaTime;
            // Keep updating the upper head and lower head positon in case that the head needs to move
            upperHead.position = Vector3.Lerp(upperHead.position, upperHeadEnd.position, timeElapsed);
            lowerHead.position = Vector3.Lerp(lowerHead.position, lowerHeadEnd.position, timeElapsed);
            spawnedLaser.transform.position = firePoint.transform.position;
            yield return null;
        } while (timeElapsed < fireInTime);
        spawnedLaser.SetActive(false);
        // Close head parts
        timeElapsed = 0.0f;
        do
        {
            timeElapsed += Time.deltaTime;
            upperHead.position = Vector3.Lerp(upperHeadEnd.position, upperHeadStart.position, timeElapsed);
            lowerHead.position = Vector3.Lerp(lowerHeadEnd.position, lowerHeadStart.position, timeElapsed);
            yield return null;
        } while (timeElapsed < closeInTime);
        isAttacking = false;
    }
}
