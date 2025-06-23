using UnityEngine;

public enum StoryDisplayType
{
    FullScreen, // Hiển thị hộp thoại toàn màn hình
    Caption     // Hiển thị text ngắn trên đầu NPC
}

public class StoryTrigger : MonoBehaviour
{
    [Header("Cốt truyện")]

    [Header("Kiểu hiển thị")]
    public StoryDisplayType displayType = StoryDisplayType.FullScreen;

    [Header("Tham chiếu Caption (nếu dùng Caption)")]
    public NPCCaptionUI captionUI;
    public StoryUIManager storyUIManager; // Tham chiếu đến StoryUIManager nếu cần
    [Header("Thời gian hiển thị (nếu có)")]
    public float displayDuration = 5f; // 0 nghĩa là không tự tắt (chờ người tắt thủ công)

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            Debug.LogError($"Wrong hell");
            TriggerStory();
        }
    }

    private void TriggerStory()
    {
        switch (displayType)
        {
            case StoryDisplayType.FullScreen:
                storyUIManager.ShowStory(displayDuration);
                break;

            case StoryDisplayType.Caption:
                if (captionUI != null)
                {
                    captionUI.ShowMessage(displayDuration);
                }
                else
                {
                    Debug.LogWarning($"⚠️ Không tìm thấy NPCCaptionUI cho {gameObject.name}");
                }
                break;
        }
    }
}
