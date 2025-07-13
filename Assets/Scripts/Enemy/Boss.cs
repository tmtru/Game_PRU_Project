using UnityEngine;

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

    private Transform player;
    private Vector2 startPosition;
    private Vector2 patrolTarget;
    private float lastAttackTime;
    private float patrolTimer;
    private bool isPatrolling = true;
    private BossDirection currentDirection;

    // Animation và Visual
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Khởi tạo các giá trị cơ bản
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
        animator.speed = 0.5f;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Thiết lập patrol target đầu tiên
        SetNewPatrolTarget();
    }

    void Update()
    {
        if (health <= 0)
        {
            currentState = EnemyState.Death;
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

    void HandleIdleState(float distanceToPlayer)
    {
        // Nếu phát hiện player trong tầm
        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
            isPatrolling = false;
            return;
        }

        // Bắt đầu patrol
        patrolTimer += Time.deltaTime;
        if (patrolTimer >= patrolWaitTime)
        {
            currentState = EnemyState.Walk;
            patrolTimer = 0f;
        }
    }

    void HandleWalkState(float distanceToPlayer)
    {
        // Nếu phát hiện player
        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
            isPatrolling = false;
            return;
        }

        // Thiết lập speed cho patrol
        moveSpeed = walkSpeed;

        // Di chuyển đến patrol target
        if (isPatrolling)
        {
            MoveTowards(patrolTarget);

            // Kiểm tra đã đến target chưa
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

        // Nếu player ra khỏi tầm detection
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = EnemyState.Idle;
            isPatrolling = true;
            return;
        }

        // Nếu đủ gần để tấn công
        if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            currentState = EnemyState.Attack;
            return;
        }

        // Thiết lập speed cho chase
        moveSpeed = chaseSpeed;

        // Đuổi theo player
        MoveTowards(player.position);
    }

    void HandleAttackState(float distanceToPlayer)
    {
        // Thực hiện tấn công
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }

        // Quay về chase nếu player còn trong tầm
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

    void HandleHurtState()
    {
        // Tạm dừng 0.5 giây khi bị hurt
        if (Time.time - lastAttackTime >= 0.5f)
        {
            if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
            {
                currentState = EnemyState.Chase;
            }
            else
            {
                currentState = EnemyState.Idle;
            }
        }
    }

    void HandleDeathState()
    {
        // Xử lý khi chết
        moveSpeed = 0f;
        // Có thể thêm hiệu ứng chết, drop item, etc.
    }

    void MoveTowards(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        // Cập nhật hướng di chuyển
        UpdateDirection(direction);
    }


    void SetNewPatrolTarget()
    {
        // Tạo điểm patrol ngẫu nhiên xung quanh vị trí ban đầu
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        patrolTarget = startPosition + randomDirection * patrolDistance;
    }

    void PerformAttack()
    {
        // Logic tấn công
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                // Gây damage cho player
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    //playerController.TakeDamage(baseAttack);
                }
                    
                Debug.Log($"{enemyName} attacked player for {baseAttack} damage!");
            }
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // Cập nhật animation parameters
        bool isMoving = currentState == EnemyState.Walk || currentState == EnemyState.Chase;
        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isAttacking", currentState == EnemyState.Attack);
        animator.SetBool("isHurt", currentState == EnemyState.Hurt);
        animator.SetBool("isDead", currentState == EnemyState.Death);

        // Cập nhật speed cho animation
        if (isMoving)
        {
            float animationSpeed = currentState == EnemyState.Chase ?
                (chaseSpeed / walkSpeed) : 1f;
            animator.speed = animationSpeed;
        }
        else
        {
            animator.speed = 1f;
        }

        // FIXED: Cập nhật hướng cho animation - chỉ khi đang di chuyển
        if (isMoving)
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
                    // THAY ĐỔI: Nếu bạn đang dùng flipX, thì dirX nên là 1 cho cả Left và Right
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

    // HOẶC giải pháp khác - không dùng flipX mà dùng hoàn toàn DirectionX
    void UpdateAnimationAlternative()
    {
        if (animator == null) return;

        // Cập nhật animation parameters
        bool isMoving = currentState == EnemyState.Walk || currentState == EnemyState.Chase;
        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isAttacking", currentState == EnemyState.Attack);
        animator.SetBool("isHurt", currentState == EnemyState.Hurt);
        animator.SetBool("isDead", currentState == EnemyState.Death);

        // Cập nhật speed cho animation
        if (isMoving)
        {
            float animationSpeed = currentState == EnemyState.Chase ?
                (chaseSpeed / walkSpeed) : 1f;
            animator.speed = animationSpeed;
        }
        else
        {
            animator.speed = 1f;
        }

        // Cập nhật hướng cho animation - chỉ khi đang di chuyển
        if (isMoving)
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

    // VÀ sửa lại UpdateDirection để không dùng flipX
    void UpdateDirection(Vector2 direction)
    {
        // Xác định hướng chính dựa trên vector di chuyển
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            currentDirection = direction.x > 0 ? BossDirection.Right : BossDirection.Left;
        }
        else
        {
            currentDirection = direction.y > 0 ? BossDirection.Up : BossDirection.Down;
        }

        // KHÔNG dùng flipX nếu bạn chọn giải pháp alternative
        // if (spriteRenderer != null)
        // {
        //     spriteRenderer.flipX = currentDirection == BossDirection.Left;
        // }
    }
    // Animation Events - Thêm vào cuối script
    public void OnAttackHit()
    {
        PerformAttack();
    }

    public void OnHurtEnd()
    {
        // Reset hurt state
        if (currentState == EnemyState.Hurt)
        {
            currentState = EnemyState.Idle;
        }
    }

    public void OnDeathEnd()
    {
        // Xử lý khi animation chết kết thúc
        gameObject.SetActive(false);
        // Hoặc có thể drop items, spawn effects, etc.
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        currentState = EnemyState.Hurt;
        lastAttackTime = Time.time; // Sử dụng để tính thời gian hurt

        Debug.Log($"{enemyName} took {damage} damage! Health: {health}");

        if (health <= 0)
        {
            currentState = EnemyState.Death;
        }
    }

    // Vẽ detection range trong Scene view
    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireCircle(transform.position, detectionRange);

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCircle(transform.position, attackRange);

    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawWireCircle(startPosition, patrolDistance);
    //}
}