using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.SceneManagement;

//Allows us to use Lists. 

public class LevelManager : Singleton<LevelManager>
{
    private const int MainSceneIndex = 0;
    private int level = 1;

    public void GoToNextLevel()
    {
        var playerGameObject = GameObject.Find("Player");
        DontDestroyOnLoad(playerGameObject);
        SceneManager.LoadSceneAsync(MainSceneIndex);
        level++;
    }
    
}