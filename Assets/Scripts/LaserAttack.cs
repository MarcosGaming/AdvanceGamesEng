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

    private GameObject spawnedLaser;                // Instance of the laser prefab
    private bool isAttacking;                       // Whether the laser is being shot or not

    private bool keepShooting;                      // Whether the laser should stop being fired

    // Start is called before the first frame update
    void Start()
    {
        spawnedLaser = Instantiate(laserPrefab, firePoint.transform) as GameObject;
        spawnedLaser.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Method that is going to be called from the procedural animation controller to shoot the laser
    public void TryShootLaser()
    {
        keepShooting = true;
        // Start laser courutine if it has not started yet
        if (!isAttacking)
        {
            StartCoroutine(ShootLaser());
        }
    }

    // Method that is going to be called from the procedural animation controller to stop shooting the laser
    public void StopShootLaser()
    {
        keepShooting = false;
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
        } while (keepShooting);
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
