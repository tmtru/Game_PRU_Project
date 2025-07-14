using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
	[SerializeField] private string sceneToLoad;

	[Header("Tuỳ chọn: Đặt vị trí spawn mới trong scene tiếp theo")]
	[SerializeField] private bool setCustomSpawn = false;

	[SerializeField] private Vector3 customSpawnPosition;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			if (HasAllKeyFragments())
			{
				if (setCustomSpawn)
				{
					GameManager.Instance.SetPlayerSpawn(customSpawnPosition, sceneToLoad);
				}

				SceneManager.LoadScene(sceneToLoad);
			}
			else
			{
				Debug.LogWarning("❌ Không đủ 3 mảnh khóa để mở cửa.");
				// Có thể bật UI cảnh báo ở đây
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
