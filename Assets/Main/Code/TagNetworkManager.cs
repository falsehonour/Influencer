using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace HashtagChampion
{
    public class TagNetworkManager : NetworkManager
    {
        //[SerializeField] private GameManager gameManager;
        [SerializeField] private Spawner spawner;
        [SerializeField] private Mirror.Discovery.NetworkDiscoveryHUD discoveryHUD;
        public static TagNetworkManager instance => (TagNetworkManager)singleton;
        public RoomManager roomManager;

        public override void Start()
        {
            base.Start();
            RegisterPrefabs();  
        }

        /*public override void OnStartServer()
        {
            base.OnStartServer();
            //Debug.Log("Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }*/
         
        public override void OnServerChangeScene(string newSceneName)
        {
            base.OnServerChangeScene(newSceneName);
            if (newSceneName == onlineScene)
            {
                //Debug.Log("Active Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                StartCoroutine(InitialiseServerGameScene());
            }
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
            bool enableDiscoveryHUD = (newSceneName == offlineScene);
            //discoveryHUD.enabled = enableDiscoveryHUD;

           /* if (newSceneName == onlineScene)
            {
                //Debug.Log("Active Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
              //  StartCoroutine(InitialiseClientGameScene());
            }*/
        }

        private IEnumerator InitialiseServerGameScene()
        {

            while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != onlineScene)
            {
                yield return new WaitForSeconds(0.1f);
            }
            spawner.Initialise();
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager)
            {
                gameManager.OnServerStarted();
            }
            else
            {
                Debug.LogError("gameManager is null. cannot initialise it");
            }
        }

       /* private IEnumerator InitialiseClientGameScene()
        {

            while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != onlineScene)
            {
                yield return new WaitForSeconds(0.15f);
            }
            discoveryHUD.enabled = false;
        }*/

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