using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public static bool initialised;

    void Start()
    {
        initialised = true;
        Debug.Log("<color=yellow>GameSceneManager initialised</color>");
    }

    private void OnDestroy()
    {
        initialised = false;
        Debug.Log("<color=yellow>GameSceneManager uninitialised</color>");

    }
}
