using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamePauseManager : MonoBehaviour
{
    [Header("Pause UI References")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitGameButton;

    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        SetupButtonEvents();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ContinueGame();
            else
                PauseGame();
        }
    }

    private void SetupButtonEvents()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(QuitGame);
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        if (pausePanel != null)
            pausePanel.SetActive(true);
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);
    }

    public void ContinueGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }
}