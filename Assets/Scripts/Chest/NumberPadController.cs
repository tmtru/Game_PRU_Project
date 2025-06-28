using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberPadController : MonoBehaviour
{
    public TMP_InputField inputField;
    public string correctCode = "3715";
    public int requiredLength = 4;
    public GameObject keypadPanel;
    public Animator chestAnimator;
    public GameObject closeButton;
	public BoxCollider2D boxCollider;
	private string currentInput = "";
    private Coroutine shakeCoroutine;
    private bool isChestOpen = false;
	public GameObject itemPrefab;         // Prefab vật phẩm
	public Transform spawnPoint;
	public AudioSource chestOpening;
	public void OnNumberButtonClick(string number)
    {
        if (isChestOpen)
        {
            Debug.Log("🔒 Rương đã mở. Không thể thao tác thêm.");
            return;
        }

        if (currentInput.Length >= requiredLength)
            return;

        currentInput += number;
        inputField.text += "*";

        if (currentInput.Length == requiredLength)
        {
            CheckCode();
        }
    }

    private void CheckCode()
    {
        if (isChestOpen)
        {
            Debug.Log("🔒 Rương đã mở. Bỏ qua kiểm tra mã.");
            return;
        }

        if (currentInput == correctCode)
        {

            Debug.Log("✅ Mã đúng! Mở rương!");
            chestAnimator.SetTrigger("ChestOpen");
            isChestOpen = true;
			chestOpening.Play();
			Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
			if (boxCollider) boxCollider.isTrigger = false;
			// Ẩn bàn phím và đảm bảo game tiếp tục
			keypadPanel.SetActive(false);
            Time.timeScale = 1f;

            // Nếu muốn đảm bảo không bao giờ mở lại bàn phím, có thể disable script này
            this.enabled = false;
        }
        else
        {
            Debug.Log("❌ Sai mã. Reset.");

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                inputField.transform.localPosition = Vector3.zero;
            }

            shakeCoroutine = StartCoroutine(ShakeAndClear());
        }

        currentInput = "";
        inputField.text = "";
    }

    private IEnumerator ShakeAndClear()
    {
        Vector3 originalPos = inputField.transform.localPosition;
        float duration = 0.2f;
        float magnitude = 10f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            inputField.transform.localPosition = originalPos + new Vector3(x, 0, 0);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        inputField.transform.localPosition = originalPos;
        shakeCoroutine = null;
    }

    public void OnClosePanel()
    {
        if (isChestOpen)
        {
            Debug.Log("🔒 Rương đã mở. Tự động ẩn bàn phím.");
            keypadPanel.SetActive(false);
            Time.timeScale = 1f;
            return;
        }

        keypadPanel.SetActive(false);
        currentInput = "";
        inputField.text = "";
        Time.timeScale = 1f;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            inputField.transform.localPosition = Vector3.zero;
            shakeCoroutine = null;
        }
    }
}
