using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class GameEndUI : MonoBehaviour
{
    public static GameEndUI Instance { get; private set; }


    [SerializeField] private GameObject winTextObject;
    [SerializeField] private GameObject loseTextObject;

    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    private PlayerLogic _playerLogic; 
    private bool _gameEnded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        HideAllUI();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerLogic = playerObj.GetComponent<PlayerLogic>();
        }

        if (restartButton != null) 
            restartButton.onClick.AddListener(RestartLevel);
        
        if (quitButton != null) 
            quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (_gameEnded || _playerLogic == null) return;

        if (_playerLogic.EyeContactDuration >= 100f)
        {
            TriggerLose();
        }
    }

    public void TriggerWin()
    {
        if (_gameEnded) return;
        
        ShowEndScreen(true);
    }

    public void TriggerLose()
    {
        if (_gameEnded) return;

        ShowEndScreen(false);
    }

    private void ShowEndScreen(bool isWin)
    {
        _gameEnded = true;

        if (winTextObject != null) winTextObject.SetActive(isWin);
        if (loseTextObject != null) loseTextObject.SetActive(!isWin);

        if (restartButton != null) restartButton.gameObject.SetActive(true);
        if (quitButton != null) quitButton.gameObject.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    private void HideAllUI()
    {
        if (winTextObject != null) winTextObject.SetActive(false);
        if (loseTextObject != null) loseTextObject.SetActive(false);
        if (restartButton != null) restartButton.gameObject.SetActive(false);
        if (quitButton != null) quitButton.gameObject.SetActive(false);
    }
}