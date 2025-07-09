using UnityEngine;

public class NumberHideOnLoad : MonoBehaviour
{
	[SerializeField] private string chestID;
	private static bool hasReset = false;
	private void Start()
	{
		Debug.Log($" 🤡🤡🤡Start ");
		if (!hasReset)
		{
			PlayerPrefs.DeleteKey(chestID);
			PlayerPrefs.Save();
			hasReset = true;
			Debug.Log($"🧹 Đã reset PlayerPrefs cho chestID: {chestID}");
		}

		if (PlayerPrefs.GetInt(chestID, 0) == 1)
		{
			gameObject.SetActive(false);
			Debug.Log($"Chest {chestID} đã mở trước đó → ẩn rương");
		}
	}
}
