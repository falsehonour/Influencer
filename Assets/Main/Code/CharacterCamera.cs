using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : MonoBehaviour
{
    private Transform myTransform;
    private Transform target;
    [SerializeField] private Transform ears;
    //private Vector3 desiredOffset;
    [SerializeField] private float distanceFromTarget = 4f;
    //private Transform targetOffset;

    [SerializeField] private float lerpSpeed;
    public float distanceMultiplier = 1;

    private bool initialised = false;


    private void FixedUpdate()
    {
        if (!initialised)
        {
            return;
        }
        float deltaTime = Time.deltaTime;
        MoveTowardsTarget(ref deltaTime);
    }

    private void MoveTowardsTarget(ref float deltaTime)
    {
        // Vector3 newPosition = Vector3.Lerp(myTransform.position, target.position + targetOffset.localPosition, lerpSpeed * deltaTime);*/
        Vector3 targetPosition = target.position;
        //ears.position = targetPosition;
        Vector3 destination = targetPosition - (myTransform.forward * distanceFromTarget * distanceMultiplier);
        Vector3 newPosition = Vector3.Lerp (myTransform.position, destination, lerpSpeed * deltaTime); 
         myTransform.position = newPosition;
        //myTransform.position = target.position + targetOffset.localPosition;
        //TODO: No need to change every frame, this is here for testing purposes
       // myTransform.rotation = targetOffset.localRotation;
    }

    internal void Initialise(Transform target/*, Transform targetOffset*/)
    {
        myTransform = transform;
        this.target = target;
        //ears.parent = null;
        ears.localPosition = (distanceFromTarget * Vector3.forward);
        //this.targetOffset = targetOffset;
        myTransform.parent = null;
        initialised = true;
    }

   /* internal void Initialise(Transform target, Vector3 offset, Quaternion rotation)
    {
        myTransform = transform;
        this.target = target;
        desiredOffset = offset;
        myTransform.parent = null;
        myTransform.rotation = rotation;
    }*/
}
