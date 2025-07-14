using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Key invisibilityKey = Key.LeftShift;
    public static PlayerController Instance;
    [SerializeField] private Transform weaponCollider;

    public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool facingLeft = false;
    private bool isInvisible = false;
	private void OnEnable()
	{
		if (playerControls != null)
			playerControls.Enable();

		SceneManager.sceneLoaded += OnSceneLoaded; // Gắn hàm callback
	}

	private void OnDisable()
	{
		if (playerControls != null)
			playerControls.Disable();

		SceneManager.sceneLoaded -= OnSceneLoaded; // Gỡ hàm callback
	}

	private void Awake()
    {
        // Prevent duplicate PlayerController
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional, remove if you want to respawn Player each scene

        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    public void TeleportTo(Vector2 targetPosition)
    {
        rb.position = targetPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Va chạm với: " + collision.gameObject.name);
    }

    public Transform GetWeaponCollider()
    {
        return weaponCollider;
    }

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Nếu GameManager đã set vị trí mới thì dịch player tới đó
		if (GameManager.Instance != null)
		{
			Debug.Log("Scene loaded. Teleporting player to spawn point: " + GameManager.Instance.playerSpawnPosition);
			TeleportTo(GameManager.Instance.playerSpawnPosition);
		}
	}

}
