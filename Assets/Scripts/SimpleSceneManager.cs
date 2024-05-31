using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneManager : MonoBehaviour
{
    // Belirtilen sahneyi y�kle
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Mevcut sahneyi yeniden y�kle
    public void RestartCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // Ana men�ye d�n
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

