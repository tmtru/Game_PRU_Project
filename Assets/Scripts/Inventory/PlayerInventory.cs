using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
	public List<InventoryItem> items = new List<InventoryItem>();
	public InventoryUI inventoryUI;

	public void AddItem(string itemName, Sprite icon)
	{
		Debug.Log($"[PlayerInventory] Thêm item: {itemName}");

		// Tìm item đã có
		InventoryItem existing = items.Find(i => i.itemName == itemName);
		if (existing != null)
		{
			existing.quantity++;
		}
		else
		{
			items.Add(new InventoryItem(itemName, icon));
		}

		if (inventoryUI != null)
		{
			inventoryUI.Refresh(items);
		}
		else
		{
			Debug.LogWarning("[PlayerInventory] inventoryUI chưa được gán!");
		}
	}

	public void UseItem(string itemName)
	{
		InventoryItem item = items.Find(i => i.itemName == itemName);
		if (item != null && item.quantity > 0)
		{
			item.quantity--;

			Debug.Log($"[PlayerInventory] Dùng item: {item.itemName}");

			if (item.quantity == 0)
				items.Remove(item);

			if (inventoryUI != null)
				inventoryUI.Refresh(items);
			else
				Debug.LogWarning("[PlayerInventory] inventoryUI chưa được gán khi UseItem!");
		}
	}
}
