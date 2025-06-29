using UnityEngine;

public class OpenPotionChest : MonoBehaviour
{
	public GameObject puzzleCanvas; // gán canvas từ inspector
	public GameObject closeButton;
	public GameObject mixButton;
	[SerializeField] private PhatronManager controller;
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") && controller.isChestOpen == false)
		{
			puzzleCanvas.SetActive(true); // mở câu đố
			mixButton.SetActive(true); // mở câu đố
			closeButton.SetActive(true); // mở câu đố
			Time.timeScale = 0f; // tạm dừng game
		}
	}
}
