using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Goblin : Enemy
{
    [Header("Movement Settings")]
    public Transform target;
    public float chaseRadius = 10f;
    public float attackRadius = 1f;
    public Transform spawnCenter;
    public float rangeWalk = 5f;
    public float speedWalk = 1f;
    public float speedRun = 3f;

    [Header("Animation")]
    public Animator animator;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    public float attackDuration = 0.8f;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    public int attackDamage = 10;
    public float knockbackForce = 5f;

    // Thêm biến để theo dõi trạng thái animation
    private bool attackAnimationCompleted = false;
    private Coroutine attackCoroutine;

    [Header("Hurt Settings")]
    public float hurtAnimationDuration = 0.6f;
    private bool isHurt = false;

    [Header("Death Settings")]
    public float deathAnimationDuration = 2f;
    public float despawnDelay = 3f;

    private Vector3 targetPosition;
    private Vector3 lastPosition;
    private Vector2 currentDirection;
    private bool isChasing = false;

    [Header("Health System")]
    public EnemyHealth enemyHealth;
    private bool isDead = false;

    void Start()
    {
        // Tìm player
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        // Thiết lập spawn center
        if (spawnCenter == null)
        {
            gameObject.name += "_withRandomWalking";
            var center = new GameObject(gameObject.name + "_center").transform;
            center.position = transform.position;
            spawnCenter = center;
        }

        // Lấy animator component
        if (animator == null)
            animator = GetComponent<Animator>();
        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        // Đăng ký event khi chết và khi bị thương
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnDeath;
            enemyHealth.OnTakeDamage += OnTakeDamage;
        }

        lastPosition = transform.position;
        ChooseNewTarget();
    }

    void Update()
    {
        // Không thực hiện hành động nào khi đang hurt hoặc chết
        if (isHurt || isDead) return;

        CheckDistance();
        UpdateAnimator();
        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();

        // Test hurt animation với phím T
        if (enemyHealth != null && Input.GetKeyDown(KeyCode.T))
        {
            TriggerHurt();
            enemyHealth.TakeDamage(10f);
            Debug.Log("Testing hurt animation with T key");
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            TriggerDeath();
            Debug.Log("Testing death animation with U key");
        }
    }

    void CheckDistance()
    {
        if (target == null || isHurt || isDead) return;

        float distanceToPlayer = Vector3.Distance(target.position, transform.position);

        if (distanceToPlayer <= attackRadius)
        {
            // Trong phạm vi tấn công
            isChasing = false;

            // Kiểm tra cooldown và bắt đầu tấn công
            if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
            {
                StartAttack();
            }
        }
        else if (distanceToPlayer <= chaseRadius)
        {
            // Đuổi theo player với tốc độ cao hơn
            isChasing = true;

            // QUAN TRỌNG: Không di chuyển khi đang attack
            if (!isAttacking)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target.position,
                    speedRun * Time.deltaTime
                );
            }
        }
        else
        {
            // Di chuyển ngẫu nhiên với tốc độ bình thường
            isChasing = false;

            // QUAN TRỌNG: Không di chuyển khi đang attack
            if (!isAttacking)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    speedWalk * Time.deltaTime
                );

                // Kiểm tra xem đã đến đích chưa
                if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
                {
                    ChooseNewTarget();
                }
            }
        }
    }

    void StartAttack()
    {
        if (isHurt || isDead || isAttacking) return;

        isAttacking = true;
        attackAnimationCompleted = false;
        lastAttackTime = Time.time;

        Debug.Log("Goblin starts attack!");

        // Bắt đầu coroutine để quản lý attack sequence
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        // Chờ một khoảng thời gian để animation bắt đầu
        yield return new WaitForSeconds(0.1f);

        // Chờ đến điểm gây damage (ví dụ: 60% animation)
        float damagePoint = attackDuration * 0.6f;
        yield return new WaitForSeconds(damagePoint - 0.1f);

        // Kiểm tra xem vẫn còn trong điều kiện attack không
        if (isAttacking && !isHurt && !isDead && target != null)
        {
            float distanceToPlayer = Vector3.Distance(target.position, transform.position);
            if (distanceToPlayer <= attackRadius)
            {
                DealDamage();
            }
        }

        // Chờ animation hoàn thành
        float remainingTime = attackDuration - damagePoint;
        yield return new WaitForSeconds(remainingTime);

        // Kết thúc attack
        EndAttack();
    }

    void OnDeath()
    {
        TriggerDeath();
    }

    public void TriggerDeath()
    {
        if (!isDead)
        {
            isDead = true;
            isAttacking = false;
            isChasing = false;
            isHurt = false;

            // Dừng tất cả Invoke và Coroutine
            CancelInvoke();
            StopAllCoroutines();

            // Bắt đầu death animation
            StartCoroutine(DeathSequence());

            Debug.Log($"{gameObject.name} is dying!");
        }
    }

    private IEnumerator DeathSequence()
    {
        DetermineDeathDirection();
        PlayDeathAnimation();

        Debug.Log($"{gameObject.name} playing death animation in direction: {GetDirectionName()}");

        yield return new WaitForSeconds(deathAnimationDuration);

        Debug.Log($"{gameObject.name} death animation completed!");

        yield return new WaitForSeconds(despawnDelay);

        DespawnGoblin();
    }

    private void DetermineDeathDirection()
    {
        if (currentDirection == Vector2.zero)
        {
            currentDirection = new Vector2(0, -1);
        }

        if (Mathf.Abs(currentDirection.x) > Mathf.Abs(currentDirection.y))
        {
            currentDirection = new Vector2(currentDirection.x > 0 ? 1 : -1, 0);
        }
        else
        {
            currentDirection = new Vector2(0, currentDirection.y > 0 ? 1 : -1);
        }
    }

    private void PlayDeathAnimation()
    {
        animator.SetFloat("MoveX", currentDirection.x);
        animator.SetFloat("MoveY", currentDirection.y);
        animator.SetBool("IsDead", true);
        animator.SetBool("IsMoving", false);
        animator.SetBool("IsChasing", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsHurt", false);
        animator.SetBool("IsRunning", false);
        animator.Play("DeathTree", 0, 0f);
    }

    private string GetDirectionName()
    {
        if (currentDirection.x > 0) return "Right";
        if (currentDirection.x < 0) return "Left";
        if (currentDirection.y > 0) return "Up";
        if (currentDirection.y < 0) return "Down";
        return "Down";
    }

    private void DespawnGoblin()
    {
        Debug.Log($"{gameObject.name} despawned!");
        Destroy(gameObject);
    }

    void OnTakeDamage(float damage, Vector3 damagePosition)
    {
        if (!isDead)
        {
            // Hủy attack đang thực hiện
            if (isAttacking)
            {
                CancelAttack();
            }
            TriggerHurt();
        }
    }

    private void CancelAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        isAttacking = false;
        attackAnimationCompleted = false;

        Debug.Log("Attack cancelled due to damage!");
    }

    public void TriggerHurt()
    {
        if (!isHurt && !isDead)
        {
            isHurt = true;

            // Hủy attack nếu đang thực hiện
            if (isAttacking)
            {
                CancelAttack();
            }

            animator.Play("HurtTree", 0, 0f);
            StartCoroutine(HurtDuration());
        }
    }

    private IEnumerator HurtDuration()
    {
        bool wasChasing = isChasing;
        isChasing = false;

        Debug.Log($"{gameObject.name} is hurt!");

        yield return new WaitForSeconds(hurtAnimationDuration);

        isHurt = false;

        if (wasChasing && target != null)
        {
            float distanceToPlayer = Vector3.Distance(target.position, transform.position);
            if (distanceToPlayer <= chaseRadius)
            {
                isChasing = true;
            }
        }

        Debug.Log($"{gameObject.name} recovered from hurt!");
    }

    public void TakeDamage(float damage, Vector3 damagePosition = default)
    {
        if (enemyHealth != null && !isDead)
        {
            enemyHealth.TakeDamage(damage, damagePosition);
        }
    }

    void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= OnDeath;
            enemyHealth.OnTakeDamage -= OnTakeDamage;
        }
    }

    void DealDamage()
    {
        if (target == null || isHurt || isDead) return;

        // Kiểm tra xem player còn trong tầm tấn công không
        float distanceToPlayer = Vector3.Distance(target.position, transform.position);
        if (distanceToPlayer > attackRadius)
        {
            Debug.Log("Player moved out of attack range!");
            return;
        }

        Debug.Log($"Goblin deals {attackDamage} damage to player!");

        // Tính toán hướng knockback
        Vector3 knockbackDirection = (target.position - transform.position).normalized;
        knockbackDirection.y = 0;

        // Áp dụng knockback
        ApplyKnockback(target, knockbackDirection);
    }

    void ApplyKnockback(Transform targetTransform, Vector3 direction)
    {
        Rigidbody targetRb = targetTransform.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
        else
        {
            targetTransform.position += direction * (knockbackForce * 0.1f);
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        attackAnimationCompleted = true;
        attackCoroutine = null;

        Debug.Log("Attack sequence completed!");
    }

    void UpdateAnimator()
    {
        if (animator == null || isDead) return;

        Vector3 deltaPosition = transform.position - lastPosition;

        // Chỉ cập nhật direction khi không đang attack để tránh làm gián đoạn animation
        if (!isAttacking && deltaPosition.magnitude > 0.01f)
        {
            if (Mathf.Abs(deltaPosition.x) > Mathf.Abs(deltaPosition.y))
            {
                currentDirection = new Vector2(deltaPosition.x > 0 ? 1 : -1, 0);
            }
            else
            {
                currentDirection = new Vector2(0, deltaPosition.y > 0 ? 1 : -1);
            }
        }

        // Cập nhật animator parameters
        animator.SetFloat("MoveX", currentDirection.x);
        animator.SetFloat("MoveY", currentDirection.y);

        // Khi đang attack, không cập nhật IsMoving
        if (!isAttacking)
        {
            animator.SetBool("IsMoving", deltaPosition.magnitude > 0.01f);
        }
        else
        {
            animator.SetBool("IsMoving", false); // Đứng yên khi attack
        }

        animator.SetBool("IsChasing", isChasing && !isAttacking);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsHurt", isHurt);
        animator.SetBool("IsDead", isDead);

        // FORCE TRANSITION cho hurt
        if (isHurt)
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsHurt", true);
        }
        else if (!isAttacking) // Chỉ cập nhật running khi không attack
        {
            if (isChasing && deltaPosition.magnitude > 0.01f)
            {
                animator.SetBool("IsRunning", true);
            }
            else
            {
                animator.SetBool("IsRunning", false);
            }
        }
        else
        {
            animator.SetBool("IsRunning", false); // Không chạy khi attack
        }

        lastPosition = transform.position;
    }

    private string GetCurrentStateName()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("IdleTree")) return "IdleTree";
        if (stateInfo.IsName("RunTree")) return "RunTree";
        if (stateInfo.IsName("AttackTree")) return "AttackTree";
        if (stateInfo.IsName("HurtTree")) return "HurtTree";
        if (stateInfo.IsName("DeathTree")) return "DeathTree";

        return "Unknown State";
    }

    void ChooseNewTarget()
    {
        int direction = Random.Range(0, 4);
        float newX = spawnCenter.position.x;
        float newZ = spawnCenter.position.z;
        float newY = transform.position.y;

        switch (direction)
        {
            case 0: newZ += rangeWalk; break;
            case 1: newZ -= rangeWalk; break;
            case 2: newX -= rangeWalk; break;
            case 3: newX += rangeWalk; break;
        }

        targetPosition = new Vector3(newX, newY, newZ);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        DrawWireCircle(transform.position, chaseRadius);

        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, attackRadius);

        if (spawnCenter != null)
        {
            Gizmos.color = Color.green;
            DrawWireCircle(spawnCenter.position, rangeWalk);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }

    void DrawWireCircle(Vector3 center, float radius, int segments = 32)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}