using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public Slider healthSlider;
    public Image fillImage;
    public Image backgroundImage;
    public Canvas healthBarCanvas;

    [Header("Visual Settings")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    [Header("Display Settings")]
    public bool showOnlyWhenDamaged = true;
    public float hideDelay = 3f; // Thời gian ẩn sau khi không bị damage
    public Vector3 offset = new Vector3(0, 2f, 0); // Offset từ enemy
    public bool faceCamera = true;

    private EnemyHealth enemyHealth;
    private Camera mainCamera;
    private float maxHealth;
    private float currentHealth;
    private bool isVisible = false;
    private float lastDamageTime;

    void Start()
    {
        // Tìm camera chính
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        // Tìm EnemyHealth component
        enemyHealth = GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogError("EnemyHealthBar: Không tìm thấy EnemyHealth component!");
            return;
        }

        // Đăng ký events
        enemyHealth.OnHealthChanged += UpdateHealthBar;
        enemyHealth.OnDeath += OnEnemyDeath;
        enemyHealth.OnDamageTaken += OnDamageTaken;

        // Thiết lập initial values
        maxHealth = enemyHealth.maxHealth;
        currentHealth = enemyHealth.currentHealth;

        // Thiết lập UI
        SetupHealthBar();

        // Ẩn healthbar ban đầu nếu cần
        if (showOnlyWhenDamaged)
        {
            SetHealthBarVisibility(false);
        }
    }

    void SetupHealthBar()
    {
        // Thiết lập canvas
        if (healthBarCanvas != null)
        {
            healthBarCanvas.worldCamera = mainCamera;
            healthBarCanvas.sortingOrder = 100; // Hiển thị trên các UI khác
        }

        // Thiết lập slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f;
            healthSlider.value = currentHealth / maxHealth;
        }

        // Thiết lập màu sắc
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
        // Cập nhật vị trí healthbar
        UpdatePosition();

        // Quay về phía camera
        if (faceCamera && mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                           mainCamera.transform.rotation * Vector3.up);
        }

        // Kiểm tra thời gian ẩn healthbar
        if (showOnlyWhenDamaged && isVisible &&
            Time.time - lastDamageTime > hideDelay)
        {
            SetHealthBarVisibility(false);
        }
    }

    void UpdatePosition()
    {
        if (transform.parent != null)
        {
            transform.position = transform.parent.position + offset;
        }
    }

    void UpdateHealthBar(float newHealth, float maxHealth)
    {
        currentHealth = newHealth;
        this.maxHealth = maxHealth;

        if (healthSlider != null)
        {
            float healthPercentage = currentHealth / maxHealth;
            healthSlider.value = healthPercentage;

            // Thay đổi màu sắc dựa trên % máu
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage);
            }
        }

        // Hiển thị healthbar khi bị damage
        if (showOnlyWhenDamaged && currentHealth < maxHealth)
        {
            SetHealthBarVisibility(true);
        }
    }

    void OnDamageTaken()
    {
        lastDamageTime = Time.time;

        if (showOnlyWhenDamaged)
        {
            SetHealthBarVisibility(true);
        }
    }

    void OnEnemyDeath()
    {
        // Ẩn healthbar khi chết
        SetHealthBarVisibility(false);
    }

    void SetHealthBarVisibility(bool visible)
    {
        isVisible = visible;

        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(visible);
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }

    void OnDestroy()
    {
        // Hủy đăng ký events
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged -= UpdateHealthBar;
            enemyHealth.OnDeath -= OnEnemyDeath;
            enemyHealth.OnDamageTaken -= OnDamageTaken;
        }
    }
}