using UnityEngine;

[System.Serializable]
public class InventoryItem
{
	public string itemName;
	public Sprite icon;
	public int quantity;
	public bool isConsumable; // ✅ true = dùng 1 lần, false = giữ lại

	public InventoryItem(string name, Sprite icon, bool isConsumable = true)
	{
		this.itemName = name;
		this.icon = icon;
		this.quantity = 1;
		this.isConsumable = isConsumable;
	}
}

