using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private int spotIndex;
    [SerializeField] private Transform[] spots;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private int extra360Rotations;

    [ContextMenu("Spin")]
    private void Spin()
    {
        StartCoroutine(SpinCoRoutine());
    }

    private IEnumerator SpinCoRoutine()
    {
        float startY = transform.eulerAngles.y;

        Vector3 endDirection = (spots[spotIndex].position - transform.position).normalized;
        Quaternion endRotation = Quaternion.LookRotation(endDirection);
              
        float endY = endRotation.eulerAngles.y + ((float)extra360Rotations * 360f);

        float endTime = curve.keys[curve.keys.Length - 1].time;
        float currentTime = 0;
        float y;
        while (currentTime < endTime)
        {
            y = Mathf.Lerp(startY, endY, curve.Evaluate(currentTime));
            transform.rotation = Quaternion.Euler(0, y, 0);
            currentTime += Time.deltaTime;
            yield return null;

        }
        Debug.Log("Spin ended");

    }
}