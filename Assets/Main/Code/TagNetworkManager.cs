using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

namespace HashtagChampion
{
    public class TagNetworkManager : NetworkManager
    {
        //[SerializeField] private GameManager gameManager;
        public static TagNetworkManager Instance => (TagNetworkManager)singleton;
        [SerializeField] private Spawner spawner;
        public static Spawner Spawner => Instance.spawner;
        [SerializeField] private RoomManager roomManager;
        public static RoomManager RoomManager => Instance.roomManager;
        [Scene]
        public string gameScene = "";
        public override void Start()
        {
            base.Start();
            RegisterPrefabs();
            Debug.Log("Network Address: " + networkAddress);
        }


        /*public override void OnStartServer()
        {
            base.OnStartServer();
            //Debug.Log("Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }*/

        public override void OnServerChangeScene(string newSceneName)
        {
            return;
           /* base.OnServerChangeScene(newSceneName);

            if (newSceneName == onlineScene)
            {
                //Debug.Log("Active Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                StartCoroutine(InitialiseServerGameScene());
            }*/
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            if(sceneName == onlineScene)
            {
                SceneManager.LoadScene(gameScene, LoadSceneMode.Additive);
            }
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);

            //discoveryHUD.enabled = enableDiscoveryHUD;

            /* if (newSceneName == onlineScene)
             {
                 //Debug.Log("Active Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                 //StartCoroutine(InitialiseClientGameScene());
             }*/
        }

       /* private IEnumerator InitialiseServerGameScene()
        {

            while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != onlineScene)
            {
                yield return new WaitForSeconds(0.1f);
                //TODO: Potential threat- what if for some reason someone tries to spawn something between these WaitForSeconds?
            }
            spawner.Initialise();
        }*/

        private void RegisterPrefabs()
        {
            GameObject[] prefabs = spawner.GetAllSpawnablePrefabs();
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject prefab = prefabs[i];
                NetworkClient.RegisterPrefab(prefab);
                //ClientScene.RegisterPrefab(prefab);
            }
        }
    }
}