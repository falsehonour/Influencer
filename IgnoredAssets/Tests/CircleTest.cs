using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleTest : MonoBehaviour
{
    [SerializeField] private int pointCount;
    [SerializeField] private float radius;

    private List<Transform> createdObjects = new List<Transform>();
    [SerializeField] private Transform circleCentre;

    [SerializeField] private GameObject pointPrefab;

    [ContextMenu("RunTest")]
    private void RunTest()
    {
        StartCoroutine(TestRoutine());
    }
    private IEnumerator TestRoutine()
    {
        foreach (Transform t in createdObjects)
        {
            Destroy(t.gameObject);
        }
        createdObjects.Clear();

        TransformStruct[] circleSpawnPoints = new TransformStruct[pointCount];
        float anglePortion = 360f / (float)pointCount;
        for (int i = 0; i < pointCount; i++)
        {
            float angle = i * anglePortion;
            Quaternion circleRotation = Quaternion.Euler(0, angle, 0);
            circleCentre.rotation = circleRotation;
           // circleCentre.Rotate(new Vector3(0, angle, 0));
            Vector3 circleForward = circleCentre.forward;
            //Debug.Log("circleForward" + circleForward);
            Vector3 position = circleCentre.position + (circleForward * radius);
            Quaternion rotation = Quaternion.LookRotation(circleForward * -1);
            // Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            circleSpawnPoints[i] = new TransformStruct(position, rotation);

        }

        for (int i = 0; i < pointCount; i++)
        {
            GameObject point = Instantiate(pointPrefab);// new GameObject("Point").transform;
            Transform pointTransform = point.transform;
            pointTransform.position = circleSpawnPoints[i].position;
            pointTransform.rotation = circleSpawnPoints[i].rotation;
            createdObjects.Add(pointTransform);
        }
        yield return new WaitForSeconds(0.1f);

    }

}
        

