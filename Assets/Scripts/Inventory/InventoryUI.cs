using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
	[Header("References")]
	public Transform slotContainer;       // GridLayoutGroup chứa các slot
	public GameObject slotPrefab;         // Prefab của InventorySlotUI

	private InventorySlotUI currentSelectedSlot;

	/// <summary>
	/// Làm mới giao diện theo danh sách vật phẩm
	/// </summary>
	public void Refresh(List<InventoryItem> items)
	{
		// Xoá tất cả các slot cũ
		foreach (Transform child in slotContainer)
		{
			Destroy(child.gameObject);
		}

		currentSelectedSlot = null;

		// Tạo lại slot mới từ danh sách item
		foreach (var item in items)
		{
			GameObject go = Instantiate(slotPrefab, slotContainer);
			var slot = go.GetComponent<InventorySlotUI>();

			if (slot != null)
			{
				slot.Setup(item.itemName, item.icon, this);
			}
			else
			{
				Debug.LogWarning("[InventoryUI] Prefab không có InventorySlotUI component!");
			}
		}
	}

	/// <summary>
	/// Được gọi bởi InventorySlotUI khi click vào slot
	/// </summary>
	public void OnSlotClicked(InventorySlotUI clickedSlot)
	{
		// Nếu slot đang được chọn → bỏ chọn
		if (currentSelectedSlot == clickedSlot)
		{
			clickedSlot.SetActive(false);
			currentSelectedSlot = null;

			InventoryManager.Instance?.ClearSelectedItem();
			Debug.Log("[InventoryUI] Bỏ chọn item");
		}
		else
		{
			// Tắt active của slot trước đó (nếu có)
			if (currentSelectedSlot != null)
				currentSelectedSlot.SetActive(false);

			// Đánh dấu slot hiện tại
			clickedSlot.SetActive(true);
			currentSelectedSlot = clickedSlot;

			InventoryManager.Instance?.SelectItem(clickedSlot.GetItemName(), clickedSlot.GetItemIcon());
			Debug.Log("[InventoryUI] Đã chọn item: " + clickedSlot.GetItemName());
		}
	}
}
