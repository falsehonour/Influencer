using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//NOTE: Imported from CakeSlicer
public class InputManager : MonoBehaviour
{
    private static Vector2 lastTouchPosition;
    private static bool isTouchDevice;

    private void Start()
    {
        isTouchDevice = (SystemInfo.deviceType == DeviceType.Handheld);
    }

    public static bool GetTouch()
    {
        return isTouchDevice ?
            ((Input.touchCount > 0) && (Input.GetTouch(0).phase != TouchPhase.Ended)) 
            : Input.GetMouseButton(0);
    }

    public static bool GetTouchDown()
    {
        return isTouchDevice ? 
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) : Input.GetMouseButtonDown(0);
    }

    public static bool GetTouchUp()
    {
        return isTouchDevice ? 
            (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended)) : Input.GetMouseButtonUp(0);
    }

    public static Vector2 GetTouchPosition()
    {

        if (isTouchDevice)
        {
            if (Input.touchCount > 0)
            {
                return lastTouchPosition = Input.GetTouch(0).position;//TODO: check what the position is on touch end
            }
        }
        else
        {
            return lastTouchPosition = Input.mousePosition;
        }
        return lastTouchPosition;//TODO: make it nullable?
    }

}
