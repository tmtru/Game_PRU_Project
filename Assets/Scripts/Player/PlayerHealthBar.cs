using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Image fillImage;
    public Image backgroundImage;
    public Canvas healthBarCanvas;

    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    public Vector3 offset = new Vector3(0, 2f, 0);
    public bool faceCamera = true;

    [SerializeField]
    private PlayerHealth playerHealth;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindAnyObjectByType<Camera>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealthBar: PlayerHealth chýa ðý?c gán!");
            return;
        }

        playerHealth.OnHealthChanged += UpdateHealthBar;
        playerHealth.OnDamageTaken += OnDamageTaken;
        playerHealth.OnDeath += OnPlayerDeath;

        SetupHealthBar();
    }

    void SetupHealthBar()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.worldCamera = mainCamera;
            healthBarCanvas.sortingOrder = 100;
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f;
            healthSlider.value = playerHealth.currentHealth / playerHealth.maxHealth;
        }

        if (fillImage != null)
        {
            fillImage.color = fullHealthColor;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }

    void Update()
    {
        transform.position = playerHealth.transform.position + offset;

        if (faceCamera && mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }

    void UpdateHealthBar(float current, float max)
    {
        float percent = current / max;
        if (healthSlider != null)
            healthSlider.value = percent;

        if (fillImage != null)
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, percent);
    }

    void OnDamageTaken()
    {
    }

    void OnPlayerDeath()
    {
        SetHealthBarVisibility(false);
        SceneManager.LoadScene("LosingScene");

    }

    void SetHealthBarVisibility(bool visible)
    {
        if (healthBarCanvas != null)
            healthBarCanvas.gameObject.SetActive(visible);
        else
            gameObject.SetActive(visible);
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
            playerHealth.OnDamageTaken -= OnDamageTaken;
            playerHealth.OnDeath -= OnPlayerDeath;
        }
    }

}
