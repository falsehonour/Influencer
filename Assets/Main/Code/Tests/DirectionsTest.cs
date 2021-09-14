using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionsTest : MonoBehaviour
{
    [SerializeField] private Transform a;
    [SerializeField] private Transform b;
    [SerializeField] private float multiplier;
    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            Test();
        }
    }

    private void Test()
    {
         Vector3 vector = (a.forward - b.forward);
         Debug.Log("vector:" + vector);
         float sqrMagnitude = vector.sqrMagnitude;
         Debug.Log("sqrMagnitude:" + sqrMagnitude.ToString());
         float normalisedSqrMagnitude = sqrMagnitude * multiplier;
         Debug.Log("normalisedSqrMagnitude:" + normalisedSqrMagnitude);
        /* Vector3 af = a.forward;
         Vector3 bf = b.forward;
         Vector3 vector = new Vector3
             (Mathf.Abs(af.x - bf.x), Mathf.Abs(af.y - bf.y), Mathf.Abs(af.z - bf.z));
         Debug.Log("vector:" + vector);
         Debug.Log("vector mag:" + vector.magnitude); */

        /*Debug.Log("a.y" + a.rotation.eulerAngles.y);
        Debug.Log("b.y" + b.rotation.eulerAngles.y);*/

    }
}
