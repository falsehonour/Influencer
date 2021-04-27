using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TagNetworkManager : NetworkManager
{
    [SerializeField] private GameManager gameManager;
    public override void OnStartServer()
    {
        base.OnStartServer();
        Spawner.Initialise();
        gameManager.OnServerStarted();
    }
}
