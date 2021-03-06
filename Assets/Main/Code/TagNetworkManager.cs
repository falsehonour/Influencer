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
        [SerializeField] private SpawnableObjectDefinitionsHolder spawnableObjectDefinitionsHolder;

        [Scene]
        public string gameScene = "";
        [Scene]
        [Tooltip("Add all sub-scenes to this list")]
        public string[] subScenes;

        [SerializeField] private PlayerController playerControllerPrefab;

       /* public override void Start()
        {
            base.Start();
            RegisterPrefabs();
        }*/

        /*public override void OnStartServer()
        {
            base.OnStartServer();

            // load all subscenes on the server only
            StartCoroutine(LoadSubScenes());
        }*/

        IEnumerator LoadSubScenes()
        {
            Debug.Log("Loading Scenes");

            foreach (string sceneName in subScenes)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                
                 Debug.Log($"Loaded {sceneName}");
            }
        }

        public override void OnStopServer()
        {
            StartCoroutine(UnloadScenes());
        }

        public override void OnStopClient()
        {
            StartCoroutine(UnloadScenes());
        }

        IEnumerator UnloadScenes()
        {
            Debug.Log("Unloading Subscenes");

            foreach (string sceneName in subScenes)
                if (SceneManager.GetSceneByName(sceneName).IsValid() || SceneManager.GetSceneByPath(sceneName).IsValid())
                {
                    yield return SceneManager.UnloadSceneAsync(sceneName);
                    // Debug.Log($"Unloaded {sceneName}");
                }

            yield return Resources.UnloadUnusedAssets();
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
                 StartCoroutine(LoadSubScenes());

               // SceneManager.LoadScene(gameScene, LoadSceneMode.Additive);
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

        private void RegisterPrefabs()
        {
            GameObject[] prefabs = spawnableObjectDefinitionsHolder.GetAllSpawnablePrefabs();
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject prefab = prefabs[i];
                NetworkClient.RegisterPrefab(prefab);
                //ClientScene.RegisterPrefab(prefab);
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            RegisterPrefabs();
        }

        public PlayerController CreatePlayerController(GameObject authority)
        {
            PlayerController playerController = Instantiate(playerControllerPrefab);
            NetworkServer.Spawn(playerController.gameObject, authority);
            //SceneManager.MoveGameObjectToScene(playerController.gameObject, SceneManager.GetSceneByPath( gameScene));
            return playerController;
        }

    }
}