using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStarter : MonoBehaviour
{
    public void StartLevel(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
