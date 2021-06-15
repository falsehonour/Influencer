using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedChecker : MonoBehaviour
{
    [SerializeField] private Transform checkedObject;
    [SerializeField] private float updateSpeed;
    [SerializeField] private float kilometresPerHour;
    private RepeatingTimer timer;
    private Vector3 previousPosition;

    void Start()
    {
        timer = new RepeatingTimer(updateSpeed);
    }

    void Update()
    {
        if (timer.Update(Time.deltaTime))
        {
            Vector3 currentPosition = checkedObject.position;
            kilometresPerHour = (Vector3.Magnitude(currentPosition - previousPosition) / updateSpeed) * 3.6f;
            previousPosition = currentPosition;
        }
    }
}
