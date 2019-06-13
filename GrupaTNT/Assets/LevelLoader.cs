using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider slider;
    public Text progressText;

    public void LoadLevel(int sceneID) {
        StartCoroutine(LoadAsynchronously(sceneID));
        LevelManager.Instance.difficultyIterator = LevelManager.Instance.difficultyIterator + 1;
        Debug.Log("Difficulty increased to: " + LevelManager.Instance.DifficultyLevel);
    }

    IEnumerator LoadAsynchronously(int sceneID) {

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);

        loadingScreen.SetActive(true);
        //operation.allowSceneActivation = false;

        while (!operation.isDone) {

            float progress = Mathf.Clamp01(operation.progress / .9f);
            //stores progress
            Debug.Log(operation.progress);
            slider.value = progress;
            progressText.text = progress * 100f + "%";
            //wait a frame
            yield return operation;
        }
        
        operation.allowSceneActivation = true;
    }
}
