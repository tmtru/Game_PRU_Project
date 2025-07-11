using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnotherMainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Button playAgainButton;
    public Button quitGameButton;

    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene";
    public string menuSceneName = "MenuScene";

    void Start()
    {
        SetupButtons();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void SetupButtons()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(() => {
                PlayAgain();
            });
        }

        if (quitGameButton != null)
        {
            quitGameButton.onClick.RemoveAllListeners();
            quitGameButton.onClick.AddListener(() => {
                QuitToMenu();
            });
        }
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    void OnDestroy()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
        }
        if (quitGameButton != null)
        {
            quitGameButton.onClick.RemoveAllListeners();
        }
    }
}