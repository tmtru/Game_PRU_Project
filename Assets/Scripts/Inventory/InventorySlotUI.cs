using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
	[Header("UI Elements")]
	public Image itemImage;         // Image con để hiển thị icon vật phẩm (gán trong "Item")
	public GameObject activeFrame;  // Khung viền để hiển thị khi đang chọn (gán trong "Active")

	private string itemName;
	private InventoryUI inventoryUI;

	/// <summary>
	/// Thiết lập dữ liệu cho slot item.
	/// </summary>
	/// <param name="itemName">Tên vật phẩm</param>
	/// <param name="icon">Sprite hiển thị</param>
	/// <param name="ui">Tham chiếu đến InventoryUI</param>
	public void Setup(string itemName, Sprite icon, InventoryUI ui)
	{
		this.itemName = itemName;
		this.inventoryUI = ui;

		if (itemImage != null)
		{
			itemImage.sprite = icon;
			itemImage.enabled = true;
		}

		SetActive(false);
	}

	/// <summary>
	/// Hàm gọi khi click vào slot.
	/// </summary>
	public void OnClickSlot()
	{
		if (inventoryUI != null)
		{
			inventoryUI.OnSlotClicked(this, itemName);
		}
		else
		{
			Debug.LogWarning("[InventorySlotUI] InventoryUI chưa được gán!");
		}
	}

	/// <summary>
	/// Bật/tắt viền active của slot.
	/// </summary>
	/// <param name="isActive">Có chọn hay không</param>
	public void SetActive(bool isActive)
	{
		if (activeFrame != null)
		{
			activeFrame.SetActive(isActive);
		}
	}
}
