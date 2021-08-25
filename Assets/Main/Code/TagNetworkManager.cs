using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace HashtagChampion
{
    public class TagNetworkManager : NetworkManager
    {
        //[SerializeField] private GameManager gameManager;
        private static TagNetworkManager instance => (TagNetworkManager)singleton;
        [SerializeField] private Spawner spawner;
        public static Spawner Spawner => instance.spawner;
        [SerializeField] private RoomManager roomManager;
        public static RoomManager RoomManager => instance.roomManager;
        [SerializeField] private Mirror.Discovery.NetworkDiscoveryHUD discoveryHUD;


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
            base.OnServerChangeScene(newSceneName);
            OnChangeScene(newSceneName);

            if (newSceneName == onlineScene)
            {
                //Debug.Log("Active Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                StartCoroutine(InitialiseServerGameScene());
            }
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
            OnChangeScene(newSceneName);


            //discoveryHUD.enabled = enableDiscoveryHUD;

            /* if (newSceneName == onlineScene)
             {
                 //Debug.Log("Active Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                 //StartCoroutine(InitialiseClientGameScene());
             }*/
        }

        public static void OnChangeScene(string newSceneName)
        {
            bool enableDiscovery = (newSceneName == instance.offlineScene) || (newSceneName == instance.onlineScene);
            instance.discoveryHUD.enabled = enableDiscovery;
        }

        private IEnumerator InitialiseServerGameScene()
        {

            while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != onlineScene)
            {
                yield return new WaitForSeconds(0.1f);
                //TODO: Potential threat- what if for some reason someone tries to spawn something between these WaitForSeconds?
            }
            spawner.Initialise();
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
            GameObject[] prefabs = spawner.GetAllSpawnablePrefabs();
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject prefab = prefabs[i];
                NetworkClient.RegisterPrefab(prefab);
                //ClientScene.RegisterPrefab(prefab);
            }
        }

        public void SwitchDiscovery()
        {
            discoveryHUD.enabled = !discoveryHUD.enabled;
        }
    }
}