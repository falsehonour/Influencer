using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame : MonoBehaviour
{
    [SerializeField]
    float openFrame;
    [SerializeField]
    float closeFrame;
    [SerializeField]
    float speed = 1;

    public bool isOpen;

    void Start()
    {
        
    }
    void Update()
    {
        if (isOpen)
        {
            OpenFrame();
        }
        else
        {
            CloseFrame();
        }
    }
    void OpenFrame()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation,
            Quaternion.Euler(transform.localRotation.x, openFrame, transform.localRotation.z), speed * Time.deltaTime);
    }

    void CloseFrame()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation,
            Quaternion.Euler(transform.localRotation.x, closeFrame, transform.localRotation.z), speed * Time.deltaTime);
    }
}
