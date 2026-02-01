using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public void Resume()
    {
        GameManager.Instance.ResumeGame();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}