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
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private ApplicationStartModes startMode;

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
        else 
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
