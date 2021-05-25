using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : MonoBehaviour
{
    private Transform myTransform;
    private Transform target;
    //private Vector3 desiredOffset;
    private Transform targetOffset;

    [SerializeField] private float lerpSpeed;
    bool Initialised
    {
        get { return target != null; }
    }

    void Update()
    {
       // myTransform.position = target.position + desiredOffset;
       /* float deltaTime = Time.deltaTime;
        MoveTowardsTarget(ref deltaTime);*/
    }

    private void FixedUpdate()
    {
        if (!Initialised)
        {
            return;
        }
        float deltaTime = Time.fixedDeltaTime;
        MoveTowardsTarget(ref deltaTime);
    }

    private void MoveTowardsTarget(ref float deltaTime)
    {
        // Vector3 currentPosition = myTransform.position;
        Vector3 newPosition = Vector3.Lerp
            (myTransform.position, target.position + targetOffset.localPosition, lerpSpeed * deltaTime);
        myTransform.position = newPosition;
        //myTransform.position = target.position + targetOffset.localPosition;
        //TODO: No need to change every frame, this is here for testing purposes
        myTransform.rotation = targetOffset.localRotation;

    }

    internal void Initialise(Transform target, Transform targetOffset)
    {
        myTransform = transform;
        this.target = target;
        this.targetOffset = targetOffset;
        myTransform.parent = null;
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
