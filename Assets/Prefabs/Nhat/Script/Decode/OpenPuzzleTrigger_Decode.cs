using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenPuzzleTrigger_Decode : MonoBehaviour
{
	public GameObject panel;
	public GameObject closeButton;
	public DecodePuzzleController manager;
	public TMP_InputField inputField;
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") && manager.isSolved == false)
		{
			panel.SetActive(true); // mở câu đố
			closeButton.SetActive(true); // mở câu đố
			Time.timeScale = 0f; // tạm dừng game
		}
	}
}
