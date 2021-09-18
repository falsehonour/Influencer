using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineStarter : MonoBehaviour
{

    private void Awake()
    {
        StaticData.Initialise();
    }

}
