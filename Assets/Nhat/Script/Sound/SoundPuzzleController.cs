using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Người chơi phải bấm đúng thứ tự các nút âm thanh ―
///     • ĐÚNG nút (ID)           ―> pass
///     • ĐÚNG thời điểm (nhịp)   ―> pass
/// Nếu sai: shake + reset.
/// Khi pass hết → mở rương.
/// </summary>
public class SoundPuzzleController : MonoBehaviour
{
	// ───────────────── UI & Scene ─────────────────
	[Header("UI References")]
	public Button[] noteButtons;           // Các nút bấm âm thanh
	public GameObject puzzlePanel;         // Panel minigame
	public GameObject closeButton;         // Nút đóng panel
	[Header("Chest / Collider")]
	public Animator chestAnimator;         // Animator của chest
	public BoxCollider2D boxCollider;      // Collider để khóa/mở chest
	// ───────────────── Âm thanh ─────────────────
	[Header("Âm thanh")]
	public AudioSource trackSource;        // AudioSource phát track gốc (nền)
	public AudioSource chestOpening;
	public AudioSource[] noteSources;      // Mỗi nút kèm 1 clip phát “bíp”

	[Header("Beat‑map (thiết lập trong Inspector)")]
	[Tooltip("Thời điểm (giây) mỗi nốt cần bấm kể từ khi track bắt đầu")]
	public List<int> noteIDs = new List<int>();

	private List<int> inputBuffer = new();
	//private double trackStartDsp;
	private bool isSolved = false;
	private Coroutine shakeCo;
	public GameObject itemPrefab;         // Prefab vật phẩm
	public Transform spawnPoint;
	void Awake()
	{
	}

	void Start()
	{
		// Tự tìm BoxCollider nếu chưa kéo
		if (boxCollider == null)
		{
			Transform chest = transform.parent.Find("Chest_2");
			if (chest) boxCollider = chest.GetComponent<BoxCollider2D>();
		}
	}
	public void StartPuzzle()
	{
		if (isSolved) return;
		//trackPlaying = true;
		puzzlePanel.SetActive(true);
		closeButton.SetActive(true);
		inputBuffer.Clear();     // ← reset
		Time.timeScale = 0f;
	}
	public void startTrack()
	{
		inputBuffer.Clear();
		trackSource.Play();
	}


	public void OnNoteButtonPressed(int id)
	{
		if (id >= 0 && id < noteSources.Length && noteSources[id])
		{
			AudioSource src = noteSources[id];
			if (!src.enabled) src.enabled = true;
			if (!src.gameObject.activeSelf) src.gameObject.SetActive(true);

			src.Stop();                      // bảo đảm phát từ đầu
			src.Play();
		}

		/* 2. Nếu đã solve → bỏ qua chấm điểm */
		if (isSolved) return;

		/* 3. Ghi ID vào buffer */
		inputBuffer.Add(id);
		Debug.Log($"Đã bấm {inputBuffer.Count}/{noteIDs.Count} – id={id}");

		/* 4. Nếu noteIDs rỗng hoặc chưa đủ 19 phím → chờ */
		if (noteIDs == null || noteIDs.Count == 0) return;
		if (inputBuffer.Count < noteIDs.Count) return;

		/* 5. Giữ lại đúng 19 phím cuối (đề phòng người chơi bấm > 19) */
		if (inputBuffer.Count > noteIDs.Count)
			inputBuffer.RemoveRange(0, inputBuffer.Count - noteIDs.Count);

		/* 6. So sánh chuỗi */
		for (int i = 0; i < noteIDs.Count; i++)
		{
			if (inputBuffer[i] != noteIDs[i])
			{
				TriggerFail();      // Sai chuỗi
				return;
			}
		}

		/* 7. Trùng khớp hoàn toàn → WIN */
		StartCoroutine(DelaySolve());
		return;
	}

	void Solve()
	{
		Debug.Log("✅ Puzzle Âm Thanh: WIN!");
		isSolved = true;

		puzzlePanel.SetActive(false);
		closeButton.SetActive(false);
		Time.timeScale = 1f;

		if (boxCollider) boxCollider.isTrigger = false;
		chestAnimator.SetTrigger("PlayAnim"); // animation mở rương
		chestOpening.Play();
		Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
	}

	void TriggerFail()
	{
		Debug.Log("❌ Sai nốt hoặc trật nhịp. Reset!");
		trackSource.Stop();

		if (shakeCo != null)
		{
			StopCoroutine(shakeCo);
			puzzlePanel.transform.localPosition = Vector3.zero;
		}
		shakeCo = StartCoroutine(ShakeAndReset());
	}

	IEnumerator ShakeAndReset()
	{
		// Lắc panel
		Vector3 origin = puzzlePanel.transform.localPosition;
		float dur = 0.25f, mag = 15f, t = 0f;
		while (t < dur)
		{
			float x = Random.Range(-1f, 1f) * mag;
			puzzlePanel.transform.localPosition = origin + Vector3.right * x;
			t += Time.unscaledDeltaTime;
			yield return null;
		}
		puzzlePanel.transform.localPosition = origin;

		// Reset lại state
		inputBuffer.Clear();
		shakeCo = null;
		// Cho phép chơi lại
		StartPuzzle();
	}

	public void OnClosePanel()
	{
		// Cho phép đóng panel giữa chừng
		puzzlePanel.SetActive(false);
		closeButton.SetActive(false);
		trackSource.Stop();
		inputBuffer.Clear();
		Time.timeScale = 1f;

		if (shakeCo != null)
		{
			StopCoroutine(shakeCo);
			puzzlePanel.transform.localPosition = Vector3.zero;
			shakeCo = null;
		}
	}
	IEnumerator DelaySolve()
	{
		isSolved = true;                    // khóa input ngay
		yield return new WaitForSecondsRealtime(1f); // chờ đúng 1 giây, không phụ thuộc Time.timeScale
		Solve();                            // gọi Win
	}
}
