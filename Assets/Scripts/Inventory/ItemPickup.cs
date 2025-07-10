using UnityEngine;

public class ItemPickup : MonoBehaviour
{
	public string itemName;
	public Sprite itemIcon;
	public bool isConsumable = true; // ✅ Gán trong Inspector (mặc định là dùng 1 lần)

	private bool isCollected = false;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (isCollected || !other.CompareTag("Player")) return;

		isCollected = true;
		Debug.Log($"[ItemPickup] Player picked up: {itemName}");

		// ✅ Ưu tiên dùng InventoryManager (Singleton)
		if (InventoryManager.Instance != null)
		{
			InventoryManager.Instance.AddItem(itemName, itemIcon, isConsumable);
		}
		else
		{
			// 🔁 Fallback về PlayerInventory (trường hợp không dùng InventoryManager)
			var inventory = other.GetComponentInParent<PlayerInventory>();
			if (inventory != null)
			{
				inventory.AddItem(itemName, itemIcon);
			}
			else
			{
				Debug.LogWarning("[ItemPickup] Không tìm thấy InventoryManager hoặc PlayerInventory.");
			}
		}

		// ✅ Hủy object sau 1 frame để tránh lỗi reference
		Invoke(nameof(DestroySelf), 0.05f);
	}

	private void DestroySelf()
	{
		Debug.Log("[ItemPickup] Destroying object: " + gameObject.name);
		Destroy(gameObject);
	}
}
