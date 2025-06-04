using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Key invisibilityKey = Key.LeftShift;

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isInvisible = false;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Update()
    {
        HandleInput();
        HandleInvisibility();
    }

    private void FixedUpdate()
    {
        Move();
        FaceDirection();
    }

    private void HandleInput()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();
        animator.SetFloat("moveX", movement.x);
        animator.SetFloat("moveY", movement.y);
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    private void FaceDirection()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
        if (worldMousePos.x < transform.position.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
    }

    private void HandleInvisibility()
    {
        if (Keyboard.current[invisibilityKey].wasPressedThisFrame)
        {
            isInvisible = !isInvisible;
            spriteRenderer.enabled = !isInvisible;
            Debug.Log("Invisibility toggled: " + isInvisible);
        }
    }

    // ✅ Gọi hàm này từ nơi khác để dịch chuyển đến vị trí bất kỳ
    public void TeleportTo(Vector2 targetPosition)
    {
        rb.position = targetPosition;
    }
}
