using UnityEngine;
using UnityEngine.UI;

public class OpenPuzzleTrigger_2 : MonoBehaviour
{
    public GameObject puzzleCanvas; // gán canvas từ inspector
    public GameObject closeButton;
	[SerializeField] private PuzzleChestController controller;
	private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && controller.isChestOpen == false)
        {
            puzzleCanvas.SetActive(true); // mở câu đố
			closeButton.SetActive(true); // mở câu đố
            Time.timeScale = 0f; // tạm dừng game
        }
    }
}
