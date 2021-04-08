using UnityEngine;

public class Cursor : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;

        transform.position = mousePosition;
    }
}
