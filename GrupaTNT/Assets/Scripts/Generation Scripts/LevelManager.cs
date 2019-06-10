using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.SceneManagement;

//Allows us to use Lists. 

public class LevelManager : Singleton<LevelManager>
{
    private int _difficultyLevel = -1;

    public int DifficultyLevel
    {
        get => _difficultyLevel / 2;
        set => _difficultyLevel = value;
    }

    public bool levelProcessing = false;
    
    private const int loadingScene = 0;
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
            _difficultyLevel++;
            Debug.Log("diff level :" + _difficultyLevel);
                
            nextScene = nextScene == loadingScene ? generationScene : loadingScene;
            levelProcessing = true;
        }
    }
}