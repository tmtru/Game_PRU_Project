using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ColorManager : MonoBehaviour
{
	[SerializeField] private string chestID = "ColorChest1";
	[SerializeField] private GameObject chestObject;

	public static ColorManager Instance;
	public List<ColorCell> cells;

	public GameObject closeButton;
	public GameObject keypadPanel;

	[SerializeField]
	private List<int> targetColorIndexes = new List<int> { 1, 0, 1, 1, 2, 0, 0, 1, 2 };
	public bool isChestOpen = false;

	public GameObject itemPrefab;
	public Transform spawnPoint;
	public AudioSource chestOpening;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
	}

	private void Start()
	{
		//if (PlayerPrefs.GetInt(chestID, 0) == 1)
		//{
		//	Debug.Log($"PlayerPrefs = 1 từ Start cho {chestID} → ẩn rương");
		//	HideChest();
		//}
		//Debug.Log("Start Called");
	}
	public void CheckWinCondition()
	{
		if (cells.Count == 0 || targetColorIndexes.Count != cells.Count)
		{
			Debug.LogWarning("Thiếu dữ liệu màu hoặc số lượng không khớp.");
			return;
		}

		for (int i = 0; i < cells.Count; i++)
		{
			if (cells[i].colorIndex != targetColorIndexes[i])
			{
				return;
			}
		}

		isChestOpen = true;
		Debug.Log("🎉 Bạn đã thắng!");
		chestOpening.Play();
		Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);

		PlayerPrefs.SetInt(chestID, 1);
		PlayerPrefs.Save();
		Time.timeScale = 1f;

		// Ẩn rương ngay lập tức
		HideChest();

		// Nếu muốn delay 5 giây, bỏ comment dòng dưới và comment HideChest()
		// StartCoroutine(DestroyChestAfterDelay());
	}

	public void OnClosePanel()
	{
		closeButton.SetActive(false);
		keypadPanel.SetActive(false);
		Time.timeScale = 1f;
	}

	private IEnumerator DestroyChestAfterDelay()
	{
		Debug.Log("Bắt đầu coroutine DestroyChestAfterDelay...");
		yield return new WaitForSecondsRealtime(5f);
		Debug.Log("Sau 5 giây, gọi HideChest()");
		HideChest();
	}

	public void HideChest()
	{
		if (chestObject != null)
		{
			chestObject.SetActive(false);
			Debug.Log($"Ẩn rương thành công cho chestID: {chestID}");
		}
		else
		{
			chestObject = GameObject.Find("Chest_Color");
			if (chestObject != null)
			{
				chestObject.SetActive(false);
				Debug.Log($"Tìm thấy Chest_Color và ẩn cho chestID: {chestID}");
			}
			else
			{
				Debug.LogWarning($"chestObject null và không tìm thấy Chest_Color cho chestID: {chestID}");
			}
		}
	}

}