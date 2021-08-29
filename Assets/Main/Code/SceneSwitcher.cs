using Mirror;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HashtagChampion
{
    public class SceneSwitcher : MonoBehaviour
    {
        public static SceneSwitcher instance;
        [SerializeField] GameObject mainMenuParent;

        private void Start()
        {
            instance = this;
        }

        private void Update()
        {

          /*  SceneManager.GetSceneAt()
            bool sceneIsHere = SceneManager.GetSceneByName(TagNetworkManager.Instance.gameScene) != null;
            Debug.Log("sceneIsHere: " + sceneIsHere);*/
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
            if (!NetworkServer.active)
            {
                //SceneManager.LoadScene(TagNetworkManager.Instance.gameScene, LoadSceneMode.Additive);
            }
            mainMenuParent.SetActive(false);
        }

        public void GoToMainMenu()
        {
            if (!NetworkServer.active)
            {
                //TODO: Maybe put the main menu in a seperate scene and load it?
                SceneManager.UnloadSceneAsync(TagNetworkManager.Instance.gameScene);
                /*MatchMakingUI.instance.ShowUI(true);
                HostUI.instance.ShowUI(false);*/
            }
            mainMenuParent.SetActive(true);

        }
    }
}

   
