using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BossDirection
{
    Up,
    Down,
    Left,
    Right
}

public class Boss : Enemy
{
    [Header("Boss Settings")]
    public float detectionRange = 10f;
    public float attackRange = 3f;
    public float attackCooldown = 2f;
    public int maxHealth = 100;

    [Header("Movement")]
    public float patrolDistance = 5f;
    public float patrolWaitTime = 2f;
    public float walkSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Attack Settings")]
    public float attackDuration = 1.2f;
    public int attackDamage = 25;
    public float knockbackForce = 8f;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    [Header("Hurt Settings")]
    public float hurtAnimationDuration = 0.8f;
    public float hurtKnockbackForce = 5f;
    public float hurtInvulnerabilityTime = 1f;
    private bool isHurt = false;
    private bool isInvulnerable = false;
    private float hurtTimer = 0f;

    [Header("Death Settings")]
    public float deathAnimationDuration = 2f; // Thời gian animation death
    public float deathFadeTime = 1f; // Thời gian fade out
    private bool isDead = false;
    private float deathTimer = 0f;

    [Header("Debug Keys")]
    public KeyCode debugStateKey = KeyCode.B;
    public KeyCode testHurtKey = KeyCode.H;
    public KeyCode testDeathKey = KeyCode.K;
    public KeyCode healKey = KeyCode.R;

    private float originalMoveSpeed;
    private Transform player;
    private Vector2 startPosition;
    private Vector2 patrolTarget;
    private float patrolTimer;
    private bool isPatrolling = true;
    private BossDirection currentDirection;
    private Vector2 lastKnockbackDirection;

    // Animation và Visual
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header("Boss Settings")]
    public bool isBoss = false;
    public GameObject fireworksPrefab;
    public AudioClip victorySound;
    public float sceneTransitionDelay = 3f;
    public string nextSceneName = "Win";

    private bool bossDeathTriggered = false;

    // Method trigger hiệu ứng khi boss chết
    void TriggerBossDeathEffects()
    {
        if (bossDeathTriggered) return;
        bossDeathTriggered = true;

        // Phát âm thanh victory
        if (victorySound != null)
        {
            AudioSource.PlayClipAtPoint(victorySound, transform.position);
        }

        // Spawn pháo hoa
        StartCoroutine(SpawnFireworks());

        // Lên lịch chuyển scene
        StartCoroutine(TransitionToNextScene());
    }

    // Coroutine spawn pháo hoa
    IEnumerator SpawnFireworks()
    {
        int fireworkCount = 5;
        for (int i = 0; i < fireworkCount; i++)
        {
            if (fireworksPrefab != null)
            {
                // Spawn pháo hoa ở vị trí ngẫu nhiên xung quanh boss
                Vector3 spawnPos = transform.position + new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-2f, 4f),
                    0f
                );

                Instantiate(fireworksPrefab, spawnPos, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    // Coroutine chuyển scene
    IEnumerator TransitionToNextScene()
    {
        yield return new WaitForSeconds(sceneTransitionDelay);

        // Fade out màn hình (optional)
        //FadeManager.Instance.FadeOut();

        // Chuyển scene
        SceneManager.LoadScene(nextSceneName);
    }
    void Start()
    {
        originalMoveSpeed = moveSpeed;
        health = maxHealth;
        currentState = EnemyState.Idle;
        startPosition = transform.position;

        // Tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Lấy components
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 0.5f;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearDamping = 5f;
            rb.freezeRotation = true;
        }

        SetNewPatrolTarget();
    }

    void Update()
    {
        // Test Keys
        HandleTestKeys();

        // Kiểm tra trạng thái chết
        if (health <= 0 && !isDead)
        {
            TriggerDeath();
        }

        // Xử lý death state
        if (isDead)
        {
            HandleDeathState();
            return;
        }

        // Xử lý hurt state
        if (isHurt)
        {
            HandleHurtState();
            return;
        }

        // Kiểm tra khoảng cách với player
        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;

        // State machine
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState(distanceToPlayer);
                break;

            case EnemyState.Walk:
                HandleWalkState(distanceToPlayer);
                break;

            case EnemyState.Chase:
                HandleChaseState(distanceToPlayer);
                break;

            case EnemyState.Attack:
                HandleAttackState(distanceToPlayer);
                break;

            case EnemyState.Hurt:
                HandleHurtState();
                break;

            case EnemyState.Death:
                HandleDeathState();
                break;
        }

        // Cập nhật animation
        UpdateAnimation();
    }

    void HandleTestKeys()
    {
        // Debug state info
        if (Input.GetKeyDown(debugStateKey))
        {
            float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;
            Debug.Log($"=== BOSS DEBUG INFO ===");
            Debug.Log($"State: {currentState}");
            Debug.Log($"Health: {health}/{maxHealth}");
            Debug.Log($"IsAttacking: {isAttacking}");
            Debug.Log($"IsHurt: {isHurt}");
            Debug.Log($"IsDead: {isDead}");
            Debug.Log($"IsInvulnerable: {isInvulnerable}");
            Debug.Log($"Distance to Player: {distanceToPlayer:F2}");
            Debug.Log($"Current Direction: {currentDirection}");
            Debug.Log($"Move Speed: {moveSpeed}");
        }

        // Test hurt
        if (Input.GetKeyDown(testHurtKey) && !isDead)
        {
            Debug.Log("Testing Hurt State...");
            TriggerHurt();
        }

        // Test death
        if (Input.GetKeyDown(testDeathKey) && !isDead)
        {
            Debug.Log("Testing Death State...");
            health = 0;
            TriggerDeath();
        }

        // Heal boss
        if (Input.GetKeyDown(healKey) && !isDead)
        {
            health = maxHealth;
            isDead = false;
            isHurt = false;
            isInvulnerable = false;
            currentState = EnemyState.Idle;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
            Debug.Log("Boss healed to full health!");
        }
    }

    void HandleIdleState(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
            isPatrolling = false;
            return;
        }

        patrolTimer += Time.deltaTime;
        if (patrolTimer >= patrolWaitTime)
        {
            currentState = EnemyState.Walk;
            patrolTimer = 0f;
        }
    }

    void HandleWalkState(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
            isPatrolling = false;
            return;
        }

        moveSpeed = walkSpeed;

        if (isPatrolling)
        {
            MoveTowards(patrolTarget);

            if (Vector2.Distance(transform.position, patrolTarget) < 0.5f)
            {
                currentState = EnemyState.Idle;
                SetNewPatrolTarget();
            }
        }
    }

    void HandleChaseState(float distanceToPlayer)
    {
        if (player == null) return;

        if (distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = EnemyState.Idle;
            isPatrolling = true;
            return;
        }

        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartAttack();
            return;
        }

        if (!isAttacking)
        {
            moveSpeed = chaseSpeed;
            MoveTowards(player.position);
        }
    }

    void HandleAttackState(float distanceToPlayer)
    {
        if (isAttacking)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, originalMoveSpeed * 0.2f, Time.deltaTime * 5f);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, originalMoveSpeed, Time.deltaTime * 3f);
        }

        if (!isAttacking)
        {
            if (distanceToPlayer <= detectionRange)
            {
                currentState = EnemyState.Chase;
            }
            else
            {
                currentState = EnemyState.Idle;
                isPatrolling = true;
            }
        }
    }

    void HandleHurtState()
    {
        // Cập nhật timer
        hurtTimer += Time.deltaTime;

        // Dừng di chuyển trong lúc hurt
        moveSpeed = 0f;

        // Áp dụng knockback nếu có
        if (hurtTimer < 0.2f) // Chỉ knockback trong 0.2s đầu
        {
            ApplyHurtKnockback();
        }

        // Kết thúc hurt state
        if (hurtTimer >= hurtAnimationDuration)
        {
            EndHurt();
        }
    }

    void HandleDeathState()
    {
        // Dừng mọi hoạt động
        moveSpeed = 0f;
        isAttacking = false;
        isHurt = false;

        // Dừng Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
        {
            animator.SetBool("isDead", true);
            if (isBoss)
            {
                TriggerBossDeathEffects();
            }
        }
        // Cập nhật timer
        deathTimer += Time.deltaTime;

        if (deathTimer >= deathAnimationDuration)
        {
            OnDeathComplete();
        }
    }

    void TriggerHurt()
    {
        if (isInvulnerable || isDead) return;

        Debug.Log($"{enemyName} is hurt!");

        isHurt = true;
        hurtTimer = 0f;
        currentState = EnemyState.Hurt;

        // Hủy attack nếu đang tấn công
        if (isAttacking)
        {
            CancelInvoke(nameof(EndAttack));
            CancelInvoke(nameof(DealDamage));
            isAttacking = false;
        }

        // Tính toán hướng knockback (từ player hoặc ngược lại)
        if (player != null)
        {
            lastKnockbackDirection = (transform.position - player.position).normalized;
        }
        else
        {
            lastKnockbackDirection = Vector2.up; // Default direction
        }

        // Bắt đầu invulnerability
        StartInvulnerability();

        // Visual feedback
        StartCoroutine(HurtFlashEffect());
    }

    void EndHurt()
    {
        isHurt = false;
        hurtTimer = 0f;
        moveSpeed = originalMoveSpeed;

        // Quyết định state tiếp theo
        if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Idle;
            isPatrolling = true;
        }

        Debug.Log($"{enemyName} recovered from hurt!");
    }

    void ApplyHurtKnockback()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(lastKnockbackDirection * hurtKnockbackForce, ForceMode2D.Force);
        }
    }

    void StartInvulnerability()
    {
        isInvulnerable = true;
        Invoke(nameof(EndInvulnerability), hurtInvulnerabilityTime);
    }

    void EndInvulnerability()
    {
        isInvulnerable = false;
        Debug.Log($"{enemyName} is no longer invulnerable");
    }

    System.Collections.IEnumerator HurtFlashEffect()
    {
        if (spriteRenderer == null) yield break;

        float flashTime = 0.1f;
        int flashCount = Mathf.RoundToInt(hurtAnimationDuration / (flashTime * 2));

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashTime);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashTime);
        }
    }

    void TriggerDeath()
    {
        if (isDead) return;

        Debug.Log($"{enemyName} is dying!");

        isDead = true;
        deathTimer = 0f;
        currentState = EnemyState.Death;

        // Hủy tất cả invoke
        CancelInvoke();

        // Dừng tất cả coroutines
        StopAllCoroutines();

        // Reset states
        isAttacking = false;
        isHurt = false;
        isInvulnerable = false;
    }

    void OnDeathComplete()
    {
        Debug.Log($"{enemyName} death complete!");
        // Có thể spawn items, effects, etc.
        // DropLoot();
        // SpawnDeathEffect();

        // Deactivate hoặc destroy
        gameObject.SetActive(false);
        // Hoặc: Destroy(gameObject);
    }

    void StartAttack()
    {
        if (isHurt || isDead) return;

        isAttacking = true;
        currentState = EnemyState.Attack;
        lastAttackTime = Time.time;

        Invoke(nameof(EndAttack), attackDuration);
        Invoke(nameof(DealDamage), attackDuration * 0.4f);

        Debug.Log($"{enemyName} starts attack!");
    }

    void DealDamage()
    {
        if (player == null || isHurt || isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"{enemyName} deals {attackDamage} damage to player!");

            Vector2 knockbackDirection = (player.position - transform.position).normalized;
            ApplyKnockback(player, knockbackDirection);
        }
        else
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Debug.Log($"{enemyName} attacked player for {attackDamage} damage!");
            }
        }
    }

    void ApplyKnockback(Transform targetTransform, Vector2 direction)
    {
        Rigidbody2D targetRb = targetTransform.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }
        else
        {
            targetTransform.position += (Vector3)(direction * (knockbackForce * 0.1f));
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        Debug.Log($"{enemyName} finished attack!");
    }

    void MoveTowards(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }

        UpdateDirection(direction);
    }

    void SetNewPatrolTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        patrolTarget = startPosition + randomDirection * patrolDistance;
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = (currentState == EnemyState.Walk || currentState == EnemyState.Chase) && !isAttacking && !isHurt;

        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isHurt", isHurt);
        animator.SetBool("isDead", isDead);
        animator.SetFloat("health", health);
        animator.SetFloat("healthPercent", (float)health / maxHealth);

        // Ưu tiên states
        if (isDead)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isHurt", false);
            animator.SetBool("isDead", true);
        }
        else if (isHurt)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isHurt", true);
            animator.SetBool("isDead", false);
        }

        // Animation speed
        if (isMoving && !isHurt && !isDead)
        {
            float animationSpeed = currentState == EnemyState.Chase ? (chaseSpeed / walkSpeed) : 1f;
            animator.speed = animationSpeed;
        }
        else
        {
            animator.speed = 1f;
        }

        // Direction cho animation
        if (isMoving && !isHurt && !isDead)
        {
            float dirX = 0f, dirY = 0f;

            switch (currentDirection)
            {
                case BossDirection.Up:
                    dirX = 0f; dirY = 1f;
                    break;
                case BossDirection.Down:
                    dirX = 0f; dirY = -1f;
                    break;
                case BossDirection.Left:
                    dirX = -1f; dirY = 0f;
                    break;
                case BossDirection.Right:
                    dirX = 1f; dirY = 0f;
                    break;
            }

            animator.SetFloat("DirectionX", dirX);
            animator.SetFloat("DirectionY", dirY);
        }
    }

    void UpdateDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            currentDirection = direction.x > 0 ? BossDirection.Right : BossDirection.Left;
        }
        else
        {
            currentDirection = direction.y > 0 ? BossDirection.Up : BossDirection.Down;
        }
    }

    // Animation Events
    public void OnAttackHit()
    {
        DealDamage();
    }

    public void OnHurtEnd()
    {
        if (isHurt) EndHurt();
    }

    public void OnDeathEnd()
    {
        OnDeathComplete();
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || isDead) return;

        health -= damage;
        health = Mathf.Max(0, health);

        Debug.Log($"{enemyName} took {damage} damage! Health: {health}/{maxHealth}");

        if (health <= 0)
        {
            TriggerDeath();
        }
        else
        {
            TriggerHurt();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Boss collided with: {collision.gameObject.name}");
    }

    //void OnDrawGizmosSelected()
    //{
    //    // Detection range
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireCircle(transform.position, detectionRange);

    //    // Attack range
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCircle(transform.position, attackRange);

    //    // Patrol area
    //    Gizmos.color = Color.blue;
    //    Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
    //    Gizmos.DrawWireCircle(startPos, patrolDistance);

    //    // Current patrol target
    //    if (Application.isPlaying && isPatrolling)
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawWireSphere(patrolTarget, 0.5f);
    //        Gizmos.DrawLine(transform.position, patrolTarget);
    //    }
    //}
}