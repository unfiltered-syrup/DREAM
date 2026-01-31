using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private bool isPaused;
    private static DateTime startTime { get; set; }
    private float pausedTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        ResetLevel();
    }

    public void Button_MainMenu()
    {
        LoadSceneManager.Instance.StartCoroutine(LoadSceneManager.TransitionToNextScene("MainMenu"));
    }

    public void Button_Restart()
    {
        LoadSceneManager.Instance.StartCoroutine(LoadSceneManager.TransitionToNextScene("Game"));
    }

    public void Button_Exit()
    {
        Application.Quit();
    }

    private void ResetLevel()
    {
        isPaused = false;
        Time.timeScale = 1;
        pausedTime = 0f;
        
        startTime = DateTime.Now;
    }

    public void EndGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        var gameTime = (float)(DateTime.Now - startTime).TotalSeconds - pausedTime;
        TimeSpan duration = TimeSpan.FromSeconds(gameTime);
        
        string formatted = string.Format("{0:D2}m {1:D2}s {2:D3}ms",
            duration.Minutes,
            duration.Seconds,
            duration.Milliseconds);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
        
        if (!isPaused) return;
        pausedTime += Time.deltaTime;
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }
}
