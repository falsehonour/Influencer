using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : MonoBehaviour
{
    private Transform myTransform;
    private Transform target;
    private Vector3 desiredOffset;
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
            (myTransform.position, target.position + desiredOffset, lerpSpeed * deltaTime);
        myTransform.position = newPosition;
    }

    internal void Initialise(Transform target, Vector3 offset)
    {
        myTransform = transform;
        this.target = target;
        desiredOffset = offset;
        myTransform.parent = null;
    }
}
