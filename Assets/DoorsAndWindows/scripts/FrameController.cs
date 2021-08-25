using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameController : MonoBehaviour
{
    public float distance = 2f;
    void Start()
    {
        
    }

   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distance))
            {
                if (hit.collider.tag == "frame")
                {
                    Frame frame = hit.collider.GetComponent<Frame>();
                    frame.isOpen = !frame.isOpen;
                }
            }
        }
    }
}
