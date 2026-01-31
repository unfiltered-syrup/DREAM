using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager Instance { get; private set; }
    [SerializeField] private Canvas loader;
    [SerializeField] private Image progressFill;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public static IEnumerator TransitionToNextScene(string nextSceneName)
    {
        Instance.loader.enabled = true;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Instance.progressFill.fillAmount = progress;
            yield return null;
        }
        Instance.loader.enabled = false;
    }
}
