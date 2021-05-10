using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionsTest : MonoBehaviour
{
    [SerializeField] private Transform a;
    [SerializeField] private Transform b;

    // Start is called before the first frame update
    void Start()
    {
        
    }

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
        Debug.Log("vector mag:" + vector.sqrMagnitude);

    }
}
