using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Một slot hiển thị item trong inventory.
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
	[Header("UI Elements")]
	[SerializeField] private Image itemImage;         // Hình ảnh vật phẩm
	[SerializeField] private GameObject activeFrame;  // Viền hiển thị khi được chọn

	private string itemName;
	private Sprite itemIcon;
	private InventoryUI inventoryUI;

	/// <summary>
	/// Thiết lập dữ liệu cho slot (gọi khi tạo mới).
	/// </summary>
	public void Setup(string name, Sprite icon, InventoryUI ui)
	{
		itemName = name;
		itemIcon = icon;
		inventoryUI = ui;

		if (itemImage != null)
		{
			itemImage.sprite = icon;
			itemImage.enabled = true;
		}
		else
		{
			Debug.LogWarning("[InventorySlotUI] itemImage chưa được gán trong Inspector.");
		}

		SetActive(false);
	}

	/// <summary>
	/// Gọi từ Button khi click vào slot này.
	/// </summary>
	public void OnClickSlot()
	{
		if (inventoryUI != null)
		{
			inventoryUI.OnSlotClicked(this);
		}
		else
		{
			Debug.LogWarning("[InventorySlotUI] InventoryUI chưa được gán.");
		}
	}

	/// <summary>
	/// Hiện/ẩn khung active viền.
	/// </summary>
	public void SetActive(bool isActive)
	{
		if (activeFrame != null)
		{
			activeFrame.SetActive(isActive);
		}
		else
		{
			Debug.LogWarning("[InventorySlotUI] activeFrame chưa được gán trong Inspector.");
		}
	}

	/// <summary>
	/// Trả về tên item của slot này.
	/// </summary>
	public string GetItemName() => itemName;

	/// <summary>
	/// Trả về icon của item.
	/// </summary>
	public Sprite GetItemIcon() => itemIcon;
}
