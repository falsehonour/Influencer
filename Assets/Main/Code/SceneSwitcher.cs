using Mirror;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HashtagChampion
{
    public class SceneSwitcher : MonoBehaviour
    {
        public static SceneSwitcher instance;
        [SerializeField]private GameObject mainMenuPrefab;
        private GameObject mainMenu;

        private void Start()
        {
            //TODO: Very lazy.. We are adding this audio listener just so we don't get no debug.logs..
            if (Mirror.NetworkServer.active)
            {
                gameObject.AddComponent<AudioListener>();
            }
            else
            {
                GoToMainMenu();          
            }
            instance = this;
        }

        public bool IsGameSceneLoaded()
        {
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                if(SceneManager.GetSceneAt(i).path == TagNetworkManager.Instance.gameScene)
                {
                    return true;
                }
            }
            return false;
        }

        public void GoToGame()
        {
            Debug.Log("GoToGame()");
            /* if (!NetworkServer.active)
             {
                 //SceneManager.LoadScene(TagNetworkManager.Instance.gameScene, LoadSceneMode.Additive);
             }*/
            Destroy(mainMenu);
            //mainMenu.SetActive(false);
        }

        public void GoToMainMenu()
        {
            Debug.Log("GoToMainMenu()");

            /*if (!NetworkServer.active)
            {
                //TODO: Maybe put the main menu in a seperate scene and load it?
                SceneManager.UnloadSceneAsync(TagNetworkManager.Instance.gameScene);
            }*/
            mainMenu = Instantiate(mainMenuPrefab);
            //mainMenu.SetActive(true);

        }
    }
}

   
