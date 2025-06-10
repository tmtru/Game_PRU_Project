using UnityEngine;

public class OpenPuzzleTrigger_1 : MonoBehaviour
{
    public GameObject puzzleCanvas; // gán canvas từ inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            puzzleCanvas.SetActive(true); // mở câu đố
            Time.timeScale = 0f; // tạm dừng game
        }
    }
}
