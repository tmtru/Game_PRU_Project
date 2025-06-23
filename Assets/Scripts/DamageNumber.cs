using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [Header("Animation Settings")]
    public float lifetime = 1.5f;
    public float moveSpeed = 2f;
    public Vector3 moveDirection = Vector3.up;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Visual Settings")]
    public bool randomizeDirection = true;
    public float randomRange = 0.5f;

    private TextMeshProUGUI textMesh;
    private Text legacyText;
    private CanvasGroup canvasGroup;
    private float timer = 0f;
    private Vector3 startPosition;
    private Vector3 initialScale;

    void Start()
    {
        // Tìm text component
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh == null)
            legacyText = GetComponent<Text>();

        // Tìm canvas group cho alpha control
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Lưu vị trí và scale ban đầu
        startPosition = transform.position;
        initialScale = transform.localScale;

        // Randomize direction nếu cần
        if (randomizeDirection)
        {
            moveDirection += new Vector3(
                Random.Range(-randomRange, randomRange),
                0,
                Random.Range(-randomRange, randomRange)
            );
            moveDirection.Normalize();
        }

        // Tự hủy sau lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / lifetime;

        // Di chuyển
        transform.position = startPosition + moveDirection * moveSpeed * timer;

        // Scale animation
        float scaleMultiplier = scaleCurve.Evaluate(progress);
        transform.localScale = initialScale * scaleMultiplier;

        // Alpha animation
        float alpha = alphaCurve.Evaluate(progress);
        canvasGroup.alpha = alpha;
    }

    public void Initialize(float damage, Color color)
    {
        // Set damage text
        string damageText = Mathf.RoundToInt(damage).ToString();

        if (textMesh != null)
        {
            textMesh.text = damageText;
            textMesh.color = color;
        }
        else if (legacyText != null)
        {
            legacyText.text = damageText;
            legacyText.color = color;
        }
    }

    public void Initialize(string text, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
        }
        else if (legacyText != null)
        {
            legacyText.text = text;
            legacyText.color = color;
        }
    }
}