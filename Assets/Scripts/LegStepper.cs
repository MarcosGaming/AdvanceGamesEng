using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegStepper : MonoBehaviour
{
    // The position and rotation to stay in range with
    [SerializeField] Transform homeTransform;
    // Max distance from home
    [SerializeField] float stepDistance;
    // How long takes for a step to be completed
    [SerializeField] float moveDuration;
    // Bool that chekcs if the leg is moving
    public bool Moving;
    // Fraction of the max distance from to overshoot
    [SerializeField] float stepOverShootFraction;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // This function is called in gecko controller so all legs dont move at the same time
    public void TryMove()
    {
        // If already moving not start another move
        if (Moving) return;

        float distFromHome = Vector3.Distance(transform.position, homeTransform.position);
        // If we are too far in position
        if (distFromHome > stepDistance)
        {
            // Start the step movement coroutine
            StartCoroutine(Move());
        }
    }

    // Coroutine for the leg movement
    IEnumerator Move()
    {
        // Leg is moving
        Moving = true;

        // Store initial conditions
        Quaternion startRot = transform.rotation;
        Vector3 startPos = transform.position;
        // End rotation is going to be the one from the home transform
        Quaternion endRot = homeTransform.rotation;

        // Vector from the foot to the home position
        Vector3 towardsHome = (homeTransform.position - transform.position);
        // Total distance to overshoot by
        float overshootDistance = stepDistance * stepOverShootFraction;
        Vector3 overshootVector = towardsHome * overshootDistance;
        // Restrict the overshoot vector to the xz plane
        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);

        // Apply the overshoot
        Vector3 endPos = homeTransform.position + overshootVector;

        // Get center between start and end positions
        Vector3 centerPos = (startPos + endPos) / 2;
        // Lift the centrePos
        centerPos += homeTransform.up * Vector3.Distance(startPos, endPos) / 2.0f;

        // Time since step started 
        float timeElapsed = 0;

        // Movement
        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / moveDuration;
            // Smooth the motion using an easing function
            normalizedTime = Easing.Cubic.InOut(normalizedTime);

            // Interpolate position with Quadratic bezier curve
            transform.position = Vector3.Lerp(Vector3.Lerp(startPos, centerPos, normalizedTime), Vector3.Lerp(centerPos, endPos, normalizedTime), normalizedTime);
            // Interpolate rotation with slerp
            transform.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

            // Wait for one frame to continue the execution
            yield return null;

        } while (timeElapsed < moveDuration);

        // Movement is finished
        Moving = false;
    }
}
