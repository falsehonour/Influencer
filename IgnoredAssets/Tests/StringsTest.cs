using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringsTest : MonoBehaviour
{
    

    [ContextMenu("Test")]
    private void Test()
    {
        string a = "A";
        string b = a;
        a = "modifiedA";
        Debug.Log("a: " + a + "b: " + b);
    }
}
