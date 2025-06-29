using UnityEngine;

public class StoryPanelClose : MonoBehaviour
{
    public GameObject storyPanel;

    public void ClosePanel()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);
    }
}