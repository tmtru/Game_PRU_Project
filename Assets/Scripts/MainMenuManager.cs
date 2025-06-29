using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private TextMeshProUGUI storyTitle;
    [SerializeField] private TextMeshProUGUI storyContent;

    private void Start()
    {
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }
        SetupStory();
    }

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("SampleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void SetupStory()
    {
        if (storyTitle != null)
        {
            storyTitle.text = "MÔ TẢ GAME";
        }

        if (storyContent != null)
        {
            storyContent.text = "Em gái Trần Dương mắc bệnh lạ. Một tin nhắn ẩn danh dẫn cậu đến U Linh Thôn, nơi duy nhất có thảo dược chữa bệnh – chỉ mọc vào đêm trăng máu.\n\n" +
                               "Ngôi làng cổ ẩn sâu trong rừng, ban ngày hiền hòa, ban đêm trở nên quái dị và nguy hiểm. Dương phát hiện dân làng là tín đồ tà thần, cần hiến tế người lạ để giữ hình hài con người.\n\n" +
                               "Muốn thoát, Dương phải tìm đủ 3 mảnh chìa khóa Rạng Đông, vượt qua Người Gác Cửa – quái vật mù nghe tiếng – và ngăn nghi lễ hiến tế.";
        }
    }

    public void ToggleStoryPanel()
    {
        if (storyPanel != null)
        {
            bool isActive = storyPanel.activeSelf;
            storyPanel.SetActive(!isActive);
        }
    }
}