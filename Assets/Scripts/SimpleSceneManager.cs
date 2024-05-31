using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneManager : MonoBehaviour
{
    // Belirtilen sahneyi yükle
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Mevcut sahneyi yeniden yükle
    public void RestartCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // Ana menüye dön
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

