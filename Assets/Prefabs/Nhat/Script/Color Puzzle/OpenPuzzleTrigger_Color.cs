using UnityEngine;
using UnityEngine.UI;

public class OpenPuzzleTrigger_Color : MonoBehaviour
{
    public GameObject panel;
    public GameObject closeButton;
	public ColorManager manager;
	private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && manager.isChestOpen == false)
        {
			panel.SetActive(true); // mở câu đố
			closeButton.SetActive(true); // mở câu đố
            Time.timeScale = 0f; // tạm dừng game
        }
    }
}
