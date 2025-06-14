using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryUIManager : MonoBehaviour
{
    public static StoryUIManager Instance;

    public GameObject fullScreenPanel;
    public GameObject closeButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowStory(float duration)
    {
        StartCoroutine(ShowAndHide(duration));
    }

    private IEnumerator ShowAndHide(float duration)
    {
        fullScreenPanel.SetActive(true);
        Time.timeScale = 0f;

        if (duration > 0)
        {
            yield return new WaitForSecondsRealtime(duration);
            CloseStory();
        }
    }


    public void CloseStory()
    {
        fullScreenPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
