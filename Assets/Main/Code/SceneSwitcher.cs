using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void GoToScene(string sceneName)
    {
        GoToScene( SceneManager.GetSceneByName(sceneName));
    }

    public void GoToScene(int sceneIndex)
    {
        GoToScene(SceneManager.GetSceneByBuildIndex(sceneIndex));

    }

    private void GoToScene(Scene scene)
    {
        HashtagChampion.TagNetworkManager.OnChangeScene(scene.path);
        SceneManager.LoadScene(scene.buildIndex);

    }
}
