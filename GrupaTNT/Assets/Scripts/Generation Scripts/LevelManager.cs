using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

//Allows us to use Lists. 

public class LevelManager : Singleton<LevelManager>
{
    public int difficultyIterator = -1;

    public int DifficultyLevel
    {
        get => difficultyIterator / 2;
    }

    public bool levelProcessing = false;
    
    private const int loadingScene = 1;
    private const int generationScene = 2;

    private int nextScene = generationScene;

    private IEnumerator GoToNextLevel()
    {
        var playerGameObject = GameObject.FindWithTag("Player");
            DontDestroyOnLoad(playerGameObject);

        AsyncOperation operation = SceneManager.LoadSceneAsync(loadingScene);

        while (!operation.isDone)
        {
            yield return null;
        }
    
    }
    
    public void LoadLevel()
    {
        if (levelProcessing == false)
        {
            StartCoroutine(GoToNextLevel());
            difficultyIterator++;
            Debug.Log("diff level :" + DifficultyLevel);
                
            nextScene = nextScene == loadingScene ? generationScene : loadingScene;
            levelProcessing = true;
        }
    }
}