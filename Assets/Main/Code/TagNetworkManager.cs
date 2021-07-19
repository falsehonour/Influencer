using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace HashtagChampion
{
    public class TagNetworkManager : NetworkManager
    {
        [SerializeField] private GameManager gameManager;


        public override void Start()
        {
            base.Start();
            RegisterPrefabs();

        }
        public override void OnStartServer()
        {
            base.OnStartServer();
            Spawner.Initialise();
            StartCoroutine(InitialiseGameManager());
        }

        private IEnumerator InitialiseGameManager()
        {
            if (gameManager)
            {
                while (!gameManager.isActiveAndEnabled)
                {

                    yield return new WaitForSeconds(0.1f);

                }
                gameManager.OnServerStarted();
            }
            else
            {
                Debug.LogError("gameManager is null. cannot initialise it");
            }
        }

        private void RegisterPrefabs()
        {
            GameObject[] prefabs = Spawner.GetAllSpawnablePrefabs();
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject prefab = prefabs[i];
                NetworkClient.RegisterPrefab(prefab);
                //ClientScene.RegisterPrefab(prefab);
            }
        }
    }
}

