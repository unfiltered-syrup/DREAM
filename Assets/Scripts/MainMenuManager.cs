using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void Button_Start()
    {
        LoadSceneManager.Instance.StartCoroutine(LoadSceneManager.TransitionToNextScene("Game"));
    }
    
    public void Button_Exit()
    {
        Application.Quit();
    }
}
