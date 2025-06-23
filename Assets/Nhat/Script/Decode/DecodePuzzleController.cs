using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DecodePuzzleController : MonoBehaviour
{
	[Header("UI References")]
	public TMP_InputField inputField;   // ô hiển thị, không cần nhận phím trực tiếp

	[Header("Mã đúng")]
	public string correctCode = "HELPME";
	public BoxCollider2D boxCollider;
	[Header("Scene Objects")]
	public GameObject puzzlePanel;
	public GameObject closeButton;
	public Animator chestAnimator;
	private string buffer = string.Empty; // ký tự người chơi gõ
	private Coroutine shakeCoroutine;
	public bool isSolved = false;

	//------------------------------ Lifecycle ------------------------------
	void OnEnable()
	{
		Keyboard.current.onTextInput += OnCharInput; // bắt phím character
	}
	void OnDisable()
	{
		Keyboard.current.onTextInput -= OnCharInput;
	}

	IEnumerator Start()
	{
		// Reset giao diện & focus
		{
			if (boxCollider == null)
			{
				Transform chestTransform = transform.parent.Find("Chest_2");
				if (chestTransform != null)
				{
					boxCollider = chestTransform.GetComponent<BoxCollider2D>();
				}
			}
			yield return null;                 // chờ 1 frame
			inputField.text = string.Empty;
			inputField.caretPosition = 0;
			inputField.interactable = false;   // chỉ dùng để hiển thị
		}
	}

	void Update()
	{
		if (isSolved) return;

		// Backspace
		if (Keyboard.current.backspaceKey.wasPressedThisFrame && buffer.Length > 0)
		{
			buffer = buffer.Remove(buffer.Length - 1);
			RefreshInput();
		}
	}

	//------------------------------ Input ----------------------------------
	void OnCharInput(char c)
	{
		if (isSolved) return;
		if (!char.IsLetter(c)) return;               // chỉ nhận chữ cái

		c = char.ToUpperInvariant(c);
		buffer += c;
		RefreshInput();

		// Kiểm tra logic
		if (buffer.Length > correctCode.Length || c != correctCode[buffer.Length - 1])
		{
			TriggerFail();
			return;
		}

		if (buffer.Length == correctCode.Length)
			Solve();
	}

	void RefreshInput()
	{
		inputField.SetTextWithoutNotify(buffer);
		inputField.caretPosition = buffer.Length;
	}

	//------------------------------ Result ---------------------------------
	void Solve()
	{
		Debug.Log("✅ Mã đúng – mở cửa!");
		isSolved = true;
		puzzlePanel?.SetActive(false);
		closeButton?.SetActive(false);
		Time.timeScale = 1f;
		if (boxCollider != null)
			boxCollider.isTrigger = false;

		chestAnimator.SetTrigger("PlayAnim");
	}

	void TriggerFail()
	{
		Debug.Log("❌ Sai mã. Reset.");
		if (shakeCoroutine != null)
		{
			StopCoroutine(shakeCoroutine);
			inputField.transform.localPosition = Vector3.zero;
		}
		shakeCoroutine = StartCoroutine(ShakeAndClear());
	}

	IEnumerator ShakeAndClear()
	{
		Vector3 orig = inputField.transform.localPosition;
		float dur = 0.2f; float mag = 10f; float t = 0f;
		while (t < dur)
		{
			float x = Random.Range(-1f, 1f) * mag;
			inputField.transform.localPosition = orig + new Vector3(x, 0, 0);
			t += Time.unscaledDeltaTime;
			yield return null;
		}
		inputField.transform.localPosition = orig;
		buffer = string.Empty;
		RefreshInput();
		shakeCoroutine = null;
	}

	//------------------------------ UI Button ------------------------------
	public void OnClosePanel()
	{
		if (isSolved)
		{
			puzzlePanel?.SetActive(false);
			closeButton?.SetActive(false);
			Time.timeScale = 1f;
			return;
		}
		puzzlePanel?.SetActive(false);
		closeButton?.SetActive(false);
		buffer = string.Empty;
		RefreshInput();
		Time.timeScale = 1f;
		if (shakeCoroutine != null)
		{
			StopCoroutine(shakeCoroutine);
			inputField.transform.localPosition = Vector3.zero;
			shakeCoroutine = null;
		}
	}
}
