using UnityEngine;

public class ItemTrigger : MonoBehaviour
{
	public string requiredItemName; // VD: "Key01"
	public bool consumeItemAfterUse = true;
	public GameObject targetObjectToActivate; // VD: cửa hoặc bất kỳ thứ gì

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Player")) return;

		// Kiểm tra xem người chơi có chọn đúng item không
		if (InventoryManager.Instance != null && InventoryManager.Instance.IsItemSelected)
		{
			string selected = InventoryManager.Instance.SelectedItemName;
			if (selected == requiredItemName)
			{
				Debug.Log($"[ItemTrigger] Dùng đúng item: {selected} để kích hoạt.");

				if (targetObjectToActivate != null)
				{
					targetObjectToActivate.SetActive(true); // mở cửa chẳng hạn
				}

				if (consumeItemAfterUse)
				{
					InventoryManager.Instance.UseItem(requiredItemName); // trừ item nếu là đồ dùng 1 lần
				}

				// Sau khi dùng xong thì xóa chọn
				InventoryManager.Instance.ClearSelectedItem();
				Destroy(gameObject); // chỉ dùng 1 lần → xóa trigger
			}
			else
			{
				Debug.Log("[ItemTrigger] Sai item đang chọn.");
			}
		}
		else
		{
			Debug.Log("[ItemTrigger] Không có item nào được chọn!");
		}
	}
}
