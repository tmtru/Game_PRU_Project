using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
	[Header("References")]
	public Transform slotContainer;       // GridLayoutGroup chứa các slot
	public GameObject slotPrefab;         // Prefab của InventorySlotUI

	private InventorySlotUI currentSelectedSlot;

	public void Refresh(List<InventoryItem> items)
	{
		// Xóa tất cả slot cũ
		foreach (Transform child in slotContainer)
		{
			Destroy(child.gameObject);
		}

		// Tạo lại slot từ danh sách item
		foreach (var item in items)
		{
			GameObject go = Instantiate(slotPrefab, slotContainer);
			var slot = go.GetComponent<InventorySlotUI>();

			if (slot != null)
			{
				slot.Setup(item.itemName, item.icon, this);

				Button btn = go.GetComponent<Button>();
				if (btn != null)
				{
					btn.onClick.RemoveAllListeners(); // đảm bảo không bị gán nhiều lần
					btn.onClick.AddListener(() => OnSlotClicked(slot, item.itemName));
				}
				else
				{
					Debug.LogWarning("[InventoryUI] slotPrefab không có Button component!");
				}
			}
			else
			{
				Debug.LogWarning("[InventoryUI] Prefab không có InventorySlotUI!");
			}
		}

		// Reset slot được chọn
		currentSelectedSlot = null;
	}

	public void OnSlotClicked(InventorySlotUI clickedSlot, string itemName)
	{
		if (currentSelectedSlot != null)
			currentSelectedSlot.SetActive(false);

		clickedSlot.SetActive(true);
		currentSelectedSlot = clickedSlot;

		Debug.Log("Đã chọn item: " + itemName);

		// Nếu muốn dùng item khi click:
		// FindObjectOfType<PlayerInventory>()?.UseItem(itemName);
	}
}
