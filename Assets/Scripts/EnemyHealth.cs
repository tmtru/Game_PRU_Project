using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public event System.Action<float, Vector3> OnTakeDamage;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Visual Effects")]
    public bool showDamageNumbers = true;
    public GameObject damageNumberPrefab;
    public Color damageColor = Color.red;

    [Header("Death Settings")]
    public float deathDelay = 2f;
    public bool destroyOnDeath = true;
    public GameObject deathEffect;

    // Events
    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public event Action OnDamageTaken;

    private bool isDead = false;
    private Animator animator;

    void Start()
    {
        // Khởi tạo máu đầy
        currentHealth = maxHealth;

        // Tìm animator
        animator = GetComponent<Animator>();

        // Thông báo health ban đầu
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage, Vector3 damagePosition = default)
    {
        if (isDead) return;

        // Giảm máu
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Trigger events
        OnDamageTaken?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Hiển thị damage number
        if (showDamageNumbers)
        {
            ShowDamageNumber(damage, damagePosition);
        }

        // Kiểm tra chết
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} healed {healAmount}. Health: {currentHealth}/{maxHealth}");
    }

    void Die()
    {
        isDead = true;

        // Chạy animation chết nếu có
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Die");
        }

        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }

        // Trigger death event
        OnDeath?.Invoke();

        // Destroy object sau delay
        if (destroyOnDeath)
        {
            Destroy(gameObject, deathDelay);
        }

        Debug.Log($"{gameObject.name} died!");
    }

    void ShowDamageNumber(float damage, Vector3 position)
    {
        if (damageNumberPrefab == null) return;

        // Sử dụng vị trí damage nếu có, không thì dùng vị trí enemy
        Vector3 spawnPosition = position != Vector3.zero ? position : transform.position + Vector3.up;

        GameObject damageNumberObj = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);

        // Thiết lập damage number nếu có script
        DamageNumber damageNumberScript = damageNumberObj.GetComponent<DamageNumber>();
        if (damageNumberScript != null)
        {
            damageNumberScript.Initialize(damage, damageColor);
        }
    }

    // Getter methods
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsFullHealth()
    {
        return currentHealth >= maxHealth;
    }

    // Method để set máu trực tiếp (dùng cho testing hoặc special cases)
    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}