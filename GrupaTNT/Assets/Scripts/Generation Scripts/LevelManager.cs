using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.SceneManagement;

//Allows us to use Lists. 

public class LevelManager : Singleton<LevelManager>
{
    private const int MainSceneIndex = 0;
    private int level = 1;
    public bool levelProcessing = false;

    public void GoToNextLevel()
    {
        if (levelProcessing == false)
        {
            var playerGameObject = GameObject.FindWithTag("Player");
            DontDestroyOnLoad(playerGameObject);
            SceneManager.LoadScene(MainSceneIndex);
            level++;
            levelProcessing = true;
        }
    }
}