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
    public float attackCooldown = 1.5f; // Thời gian chờ giữa các đòn tấn công
    public float attackDuration = 0.8f;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    public int attackDamage = 10; // Sát thương gây ra
    public float knockbackForce = 5f; // Lực đẩy lùi player

    [Header("Hurt Settings")]
    public float hurtAnimationDuration = 0.6f; // Thời gian animation hurt
    private bool isHurt = false;

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
            enemyHealth.OnTakeDamage += OnTakeDamage; // Thêm event khi bị thương
        }

        // Lưu vị trí ban đầu để tính toán hướng di chuyển
        lastPosition = transform.position;

        // Chọn mục tiêu di chuyển ban đầu
        ChooseNewTarget();
    }

    void Update()
    {
        // Không thực hiện hành động nào khi đang hurt hoặc chết
        if (isHurt || isDead) return;

        CheckDistance();
        UpdateAnimator();

        // Test hurt animation với phím T
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Test trực tiếp hurt animation
            TriggerHurt();
            Debug.Log("Testing hurt animation with T key");
        }

        // Test damage qua EnemyHealth với phím Y  
        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null && Input.GetKeyDown(KeyCode.Y))
        {
            enemyHealth.TakeDamage(10f);
            Debug.Log("Testing damage with Y key");
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
            isAttacking = false;

            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                speedRun * Time.deltaTime // Dùng speedRun khi chase
            );
        }
        else
        {
            // Di chuyển ngẫu nhiên với tốc độ bình thường
            isChasing = false;
            isAttacking = false;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speedWalk * Time.deltaTime // Dùng speedWalk khi walk
            );

            // Kiểm tra xem đã đến đích chưa
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                ChooseNewTarget();
            }
        }
    }

    void StartAttack()
    {
        if (isHurt || isDead) return; // Không tấn công khi đang hurt

        isAttacking = true;
        lastAttackTime = Time.time;

        // Tự động kết thúc attack sau attackDuration
        Invoke(nameof(EndAttack), attackDuration);

        // Delay một chút để sync với animation trước khi gây damage
        Invoke(nameof(DealDamage), attackDuration * 0.4f); // Gây damage ở 40% animation

        Debug.Log("Goblin starts attack!");
    }

    void OnDeath()
    {
        isDead = true;
        isAttacking = false;
        isChasing = false;
        isHurt = false;

        // Dừng tất cả Invoke và Coroutine
        CancelInvoke();
        StopAllCoroutines();

        Debug.Log($"{gameObject.name} died!");
    }

    // Event handler khi bị thương
    void OnTakeDamage(float damage, Vector3 damagePosition)
    {
        if (!isDead)
        {
            TriggerHurt();
        }
    }

    // Trigger hurt animation
    public void TriggerHurt()
    {
        if (!isHurt && !isDead) // Tránh spam hurt animation
        {
            isHurt = true;
            animator.Play("HurtTree", 0, 0f);
            StartCoroutine(HurtDuration());
        }
    }

    private IEnumerator HurtDuration()
    {
        // Tạm dừng các hành động khác khi bị thương
        bool wasChasing = isChasing;
        bool wasAttacking = isAttacking;

        isChasing = false;
        isAttacking = false;

        // Hủy các invoke đang chờ
        CancelInvoke();

        Debug.Log($"{gameObject.name} is hurt!");

        // Đợi animation hurt hoàn thành
        yield return new WaitForSeconds(hurtAnimationDuration);

        // Reset hurt state
        isHurt = false;

        // Không khôi phục trạng thái attack vì nó đã bị gián đoạn
        // Chỉ khôi phục chase nếu player vẫn trong tầm
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
        // Hủy đăng ký event
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
        if (distanceToPlayer > attackRadius) return;

        // Tìm PlayerHealth component
        //PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        //if (playerHealth != null)
        //{
        // Gây damage
        //playerHealth.TakeDamage(attackDamage);

        Debug.Log($"Goblin deals {attackDamage} damage to player!");

        // Tính toán hướng knockback
        Vector3 knockbackDirection = (target.position - transform.position).normalized;
        knockbackDirection.y = 0; // Chỉ knockback theo mặt phẳng ngang

        // Áp dụng knockback
        ApplyKnockback(target, knockbackDirection);
        //}
        //else
        //{
        //    Debug.LogWarning("Player không có PlayerHealth component!");
        //}
    }

    void ApplyKnockback(Transform targetTransform, Vector3 direction)
    {
        // Thử tìm Rigidbody để áp dụng knockback
        Rigidbody targetRb = targetTransform.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
        else
        {
            // Nếu không có Rigidbody, di chuyển trực tiếp
            targetTransform.position += direction * (knockbackForce * 0.1f);
        }
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Tính toán hướng di chuyển dựa trên sự thay đổi vị trí
        Vector3 deltaPosition = transform.position - lastPosition;

        // Chỉ cập nhật nếu có di chuyển đáng kể
        if (deltaPosition.magnitude > 0.01f)
        {
            // Xác định hướng chính (ưu tiên hướng có giá trị tuyệt đối lớn hơn)
            if (Mathf.Abs(deltaPosition.x) > Mathf.Abs(deltaPosition.y))
            {
                // Di chuyển theo trục X (trái/phải)
                currentDirection = new Vector2(deltaPosition.x > 0 ? 1 : -1, 0);
            }
            else
            {
                // Di chuyển theo trục Z (lên/xuống)
                currentDirection = new Vector2(0, deltaPosition.y > 0 ? 1 : -1);
            }
        }

        // Debug thông tin animator
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"Current Animator State: {animator.GetCurrentAnimatorStateInfo(0).IsName("HurtTree")}");
            Debug.Log($"IsHurt parameter: {animator.GetBool("IsHurt")}");
            Debug.Log($"Current state name: {GetCurrentStateName()}");
        }

        // Cập nhật animator parameters
        animator.SetFloat("MoveX", currentDirection.x);
        animator.SetFloat("MoveY", currentDirection.y);
        animator.SetBool("IsMoving", deltaPosition.magnitude > 0.01f);
        animator.SetBool("IsChasing", isChasing);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsHurt", isHurt); // Thêm parameter cho hurt state

        // FORCE TRANSITION - Ưu tiên tuyệt đối cho hurt
        if (isHurt)
        {
            // Reset tất cả states khác khi hurt
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsMoving", false);

            // Force set hurt state
            animator.SetBool("IsHurt", true);

            Debug.Log("FORCING HURT STATE!");
        }
        else
        {
            // Logic bình thường khi không hurt
            if (isAttacking)
            {
                animator.SetBool("IsRunning", false);
            }
            else if (isChasing && deltaPosition.magnitude > 0.01f)
            {
                animator.SetBool("IsRunning", true);
            }
            else
            {
                animator.SetBool("IsRunning", false);
            }
        }

        // Lưu vị trí hiện tại cho frame tiếp theo
        lastPosition = transform.position;
    }

    // Helper method để debug animator state
    private string GetCurrentStateName()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("IdleTree")) return "IdleTree";
        if (stateInfo.IsName("RunTree")) return "RunTree";
        if (stateInfo.IsName("AttackTree")) return "AttackTree";
        if (stateInfo.IsName("HurtTree")) return "HurtTree";

        return "Unknown State";
    }

    void ChooseNewTarget()
    {
        // Di chuyển theo 4 hướng cơ bản
        int direction = Random.Range(0, 4);
        float newX = spawnCenter.position.x;
        float newZ = spawnCenter.position.z;
        float newY = transform.position.y; // Giữ nguyên độ cao

        switch (direction)
        {
            case 0: // Lên (Z+)
                newZ += rangeWalk;
                break;
            case 1: // Xuống (Z-)
                newZ -= rangeWalk;
                break;
            case 2: // Trái (X-)
                newX -= rangeWalk;
                break;
            case 3: // Phải (X+)
                newX += rangeWalk;
                break;
        }

        targetPosition = new Vector3(newX, newY, newZ);
    }

    // Gizmos để debug trong Scene view
    void OnDrawGizmosSelected()
    {
        // Vẽ bán kính chase
        Gizmos.color = Color.yellow;
        DrawWireCircle(transform.position, chaseRadius);

        // Vẽ bán kính attack
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, attackRadius);

        // Vẽ khu vực di chuyển
        if (spawnCenter != null)
        {
            Gizmos.color = Color.green;
            DrawWireCircle(spawnCenter.position, rangeWalk);
        }

        // Vẽ đích đến hiện tại
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