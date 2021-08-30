using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ApplicationStarter : MonoBehaviour
{

   private enum ApplicationStartModes
   {
       Server = 1, Client = 2, Host = 3
   }
    [SerializeField] private bool autoConnection;
    [SerializeField] private ApplicationStartModes startMode;
    [SerializeField] private NetworkManager networkManager;

    private void Awake()
    {
        //TODO: Does the server need this?
        StaticData.Initialise();
    }

    void Start()
    {
        if (Application.isBatchMode)
        { //Headless build
            Debug.Log("=== Server Build ===");
        }
        else if (autoConnection)
        {
            switch (startMode)
            {
                case ApplicationStartModes.Server:
                    {
                        networkManager.StartServer();
                    }
                    break;
                case ApplicationStartModes.Client:
                    {
                        networkManager.StartClient();
                    }
                    break;
                case ApplicationStartModes.Host:
                    {
                        networkManager.StartHost();
                    }
                    break;
            }

            Debug.Log($"=== Starting as {startMode.ToString()} ===");

        }

    }

    public void TryConnectToServer()
    {
        networkManager.StartClient();
    }

}
