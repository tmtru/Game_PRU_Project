using UnityEngine;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
	public string itemName;
	public Sprite itemIcon;

	private bool isCollected = false;

	private void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log($"[ItemPickup] Triggered by {other.name}");

		if (isCollected) return;

		if (other.CompareTag("Player"))
		{
			Debug.Log("[ItemPickup] Player confirmed");
			isCollected = true;

			// Tìm inventory từ cha nếu player có nhiều phần tử con
			var inventory = other.GetComponentInParent<PlayerInventory>();
			if (inventory != null)
			{
				inventory.AddItem(itemName, itemIcon);
				Debug.Log("[ItemPickup] Added item, scheduling destroy");

				// Cách mới: dùng Invoke để đảm bảo không bị lỗi Unity coroutine
				Invoke(nameof(DestroySelf), 0.05f);
			}
			else
			{
				Debug.LogWarning("[ItemPickup] Không tìm thấy PlayerInventory trên " + other.name);
			}
		}
	}

	private void DestroySelf()
	{
		Debug.Log("[ItemPickup] Destroying item: " + gameObject.name);
		Destroy(gameObject);
	}
}
