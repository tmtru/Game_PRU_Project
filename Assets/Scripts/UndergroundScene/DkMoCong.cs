using UnityEngine;
using UnityEngine.SceneManagement;

public class DkMoCong : MonoBehaviour
{
	public float requiredTime = 5f;
	private float timer = 0f;
	private bool playerInZone = false;
	private bool hasSacrificed = false;

	[Header("Vật phẩm cần thiết")]
	public string correctBloodItem = "Blood_Real";
	public string wrongBloodItem = "Blood_Fake";
	public string requiredExtraItem = "Potion"; // Thuốc giải

	[Header("Tên scene kết thúc")]
	public string winSceneName = "WinScene";
	public string loseSceneName = "LoseScene";

	[Header("Âm thanh")]
	public AudioSource ritualHintSound; // ✅ Âm thanh phát khi bước vào đúng vùng (chỉ 1 lần)
	private bool hintPlayed = false;

	private void Update()
	{
		if (playerInZone && !hasSacrificed)
		{
			var inventory = InventoryManager.Instance;
			var selected = inventory.SelectedItemName;

			// ✅ Trường hợp thành công
			if (selected == correctBloodItem && inventory.HasItem(requiredExtraItem))
			{
				timer += Time.deltaTime;

				if (timer >= requiredTime)
				{
					hasSacrificed = true;
					Debug.Log("✅ Hiến tế thành công – mở kết thúc tốt.");

					inventory.UseItem(correctBloodItem);
					inventory.UseItem(requiredExtraItem);
					SceneManager.LoadScene(winSceneName);
				}
			}
			// ❌ Trường hợp thất bại do chọn máu giả
			else if (selected == wrongBloodItem)
			{
				timer += Time.deltaTime;

				if (timer >= requiredTime)
				{
					hasSacrificed = true;
					Debug.Log("❌ Máu giả – kết thúc xấu.");

					inventory.UseItem(wrongBloodItem);
					SceneManager.LoadScene(loseSceneName);
				}
			}
			else
			{
				timer = 0f;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			playerInZone = true;
			timer = 0f;

			if (!hintPlayed && ritualHintSound != null)
			{
				ritualHintSound.Play();
				hintPlayed = true;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			playerInZone = false;
			timer = 0f;
		}
	}
}
