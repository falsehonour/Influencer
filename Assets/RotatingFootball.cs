using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingFootball : MonoBehaviour
{
    private Vector3 previousPosition;
    private Transform myTransform;
    [SerializeField] private float rotationSpeed;

    private void Start()
    {
        myTransform = transform;
    }

    private void FixedUpdate()
    {
        //TODO: Skip if not on screen
        Vector3 currentPosition = myTransform.position;
        float difference = (currentPosition - previousPosition).magnitude;
        myTransform.Rotate(difference * rotationSpeed, 0, 0);

        previousPosition = currentPosition;
    }

    
}
