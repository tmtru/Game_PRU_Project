using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
	[SerializeField] private string sceneToLoad;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			if (HasAllKeyFragments())
			{
				SceneManager.LoadScene(sceneToLoad);
			}
			else
			{
				Debug.LogWarning("❌ Không đủ 3 mảnh khóa để mở cửa.");
				// Gợi ý thêm: có thể hiện UI cảnh báo hoặc chơi âm thanh cảnh báo
			}
		}
	}

	private bool HasAllKeyFragments()
	{
		return InventoryManager.Instance.HasItem("Key1") &&
			   InventoryManager.Instance.HasItem("Key2") &&
			   InventoryManager.Instance.HasItem("Key3");
	}
}
