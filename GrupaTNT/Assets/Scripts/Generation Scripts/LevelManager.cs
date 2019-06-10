using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.SceneManagement;

//Allows us to use Lists. 

public class LevelManager : Singleton<LevelManager>
{
    private int _difficultyLevel = 0;

    public int DifficultyLevel
    {
        get => _difficultyLevel / 2;
        set => _difficultyLevel = value;
    }

    public bool levelProcessing = false;
    
    private const int loadingScene = 0;
    private const int generationScene = 2;

    private int nextScene = generationScene;

    public IEnumerator GoToNextLevel()
    {
        if (levelProcessing == false)
        {

            var playerGameObject = GameObject.FindWithTag("Player");
            DontDestroyOnLoad(playerGameObject);

            AsyncOperation operation = SceneManager.LoadSceneAsync(loadingScene);

            while (!operation.isDone)
            {
                yield return null;
            }

            _difficultyLevel++;
            levelProcessing = true;
            
            nextScene = nextScene == loadingScene ? generationScene : loadingScene;
        }
    }
}