using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TagNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        Spawner.Initialise();
    }
}
