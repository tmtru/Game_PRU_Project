using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
	public static InventoryManager Instance;

	public List<InventoryItem> items = new();
	public InventoryUI inventoryUI;

	private string selectedItemName;
	private Sprite selectedItemIcon;

	public string SelectedItemName => selectedItemName;
	public Sprite SelectedItemIcon => selectedItemIcon;
	public bool IsItemSelected => !string.IsNullOrEmpty(selectedItemName);

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void AddItem(string itemName, Sprite icon, bool isConsumable = true)
	{
		Debug.Log($"[InventoryManager] AddItem: {itemName}");

		InventoryItem existing = items.Find(i => i.itemName == itemName);
		if (existing != null)
		{
			existing.quantity++;
		}
		else
		{
			items.Add(new InventoryItem(itemName, icon, isConsumable));
		}

		inventoryUI?.Refresh(items);
	}

	public void UseSelectedItem()
	{
		if (!IsItemSelected) return;
		UseItem(selectedItemName);
	}

	public void UseItem(string itemName)
	{
		InventoryItem item = items.Find(i => i.itemName == itemName);
		if (item == null) return;

		// THỰC HIỆN TÁC DỤNG CỦA ITEM (tuỳ loại)
		TriggerItemEffect(item.itemName);

		if (item.isConsumable)
		{
			item.quantity--;
			if (item.quantity <= 0)
			{
				items.Remove(item);
				if (selectedItemName == itemName)
					ClearSelectedItem();
			}
		}

		inventoryUI?.Refresh(items);
	}

	public void SelectItem(string itemName, Sprite icon)
	{
		selectedItemName = itemName;
		selectedItemIcon = icon;
		Debug.Log($"[InventoryManager] Selected item: {itemName}");
	}

	public void ClearSelectedItem()
	{
		selectedItemName = null;
		selectedItemIcon = null;
		Debug.Log("[InventoryManager] Cleared selected item");
	}

	public bool HasItem(string itemName)
	{
		return items.Exists(i => i.itemName == itemName && i.quantity > 0);
	}

	// Đây là nơi bạn có thể xử lý logic từng item cụ thể
	private void TriggerItemEffect(string itemName)
	{
		if (itemName == "HealthPotion")
		{
			Debug.Log("[Effect] Hồi máu cho player");
			// player.Heal(50);
		}
		else if (itemName == "GoldenKey")
		{
			Debug.Log("[Effect] Dùng chìa khóa ở đâu đó...");
			// Dùng để mở cửa bí mật
		}
	}
}
