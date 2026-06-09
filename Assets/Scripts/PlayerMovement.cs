using UnityEngine;
using System.Collections;

/// Handles player movement, input, and the dash ability.
/// This script requires a Rigidbody2D, an Animator, and a SpriteRenderer component.
[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The normal movement speed of the player.")]
    public float speed = 5f;

    [Header("Dash Settings")]
    [Tooltip("How far the player will dash.")]
    public float dashRange = 5f;
    [Tooltip("The duration of the dash in seconds.")]
    public float dashDuration = 0.2f;
    [Tooltip("The cooldown time for the dash in seconds.")]
    public float dashCooldown = 2f;

    [Header("Dash Visual Feedback")]
    [Tooltip("Color to flash when the dash cooldown is complete.")]
    public Color dashReadyFlashColor = Color.cyan;
    [Tooltip("Duration of the readiness flash in seconds.")]
    public float flashDuration = 0.15f;

    [Header("UI References")]
    [Tooltip("Reference to the DashCooldown script on the UI icon.")]
    public DashCooldown dashIconCooldown;

    // --- Private Variables ---
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 direction;
    private bool isDashing;
    private float lastDashTime;
    private bool isDashReady;

    // Public property to check invincibility status from other scripts.
    public bool isInvincible { get; set; }

    private void Awake()
    {
        // Get component references once at the start.
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize lastDashTime to allow dashing from the start.
        lastDashTime = -dashCooldown;
        isDashReady = true;
    }

    private void Update()
    {
        if (DialogueManager.instance != null && DialogueManager.instance.isDialogueActive)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetLayerWeight(1, 0);
            direction = Vector2.zero;
            return;
        }

        CheckDashReadiness();

        // Only process input if the player is not currently dashing.
        if (!isDashing)
        {
            TakeInput();
        }
    }

    private void FixedUpdate()
    {
        // Handle physics-based movement in FixedUpdate.
        if (!isDashing)
        {
            Move();
        }
    }

    private void OnDisable()
    {
        // Reset all movement locks when the GameObject is deactivated (e.g., during respawn)
        isDashing = false;
        isInvincible = false;
        direction = Vector2.zero;

        // Reset animation states to prevent getting stuck in a walking frame
        if (animator != null)
        {
            animator.SetLayerWeight(1, 0);
        }
    }

    /// Monitors the cooldown timer and triggers the visual feedback when ready.
    private void CheckDashReadiness()
    {
        if (!isDashReady && Time.time >= lastDashTime + dashCooldown)
        {
            isDashReady = true;
            StartCoroutine(DashReadyFlashCoroutine());
        }
    }

    /// Reads player input for movement and actions like dashing.
    private void TakeInput()
    {
        direction = Vector2.zero;

        // Read directional input.
        if (Input.GetKey(KeyCode.UpArrow)) direction += Vector2.up;
        if (Input.GetKey(KeyCode.DownArrow)) direction += Vector2.down;
        if (Input.GetKey(KeyCode.LeftArrow)) direction += Vector2.left;
        if (Input.GetKey(KeyCode.RightArrow)) direction += Vector2.right;

        // Normalize the direction vector to prevent faster diagonal movement.
        if (direction.magnitude > 1)
        {
            direction.Normalize();
        }

        // Check for dash input.
        if (Input.GetKeyDown(KeyCode.Space) && direction != Vector2.zero && isDashReady)
        {
            PerformDash();
        }
    }

    /// Executes the dash ability and triggers the UI cooldown.
    private void PerformDash()
    {
        lastDashTime = Time.time;
        isDashReady = false;

        // Start the visual cooldown on the UI icon if it's assigned.
        if (dashIconCooldown != null)
        {
            dashIconCooldown.StartCooldown(dashCooldown);
        }

        StartCoroutine(DashCoroutine());
    }

    /// Moves the player based on the current direction input.
    private void Move()
    {
        // Use Rigidbody.MovePosition for smooth, physics-based movement.
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        if (direction.magnitude > 0)
        {
            SetAnimatorMovement(direction);
        }
        else
        {
            // Deactivate the movement animation layer if not moving.
            animator.SetLayerWeight(1, 0);
        }
    }

    /// The coroutine that handles the dash movement over time.
    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        isInvincible = true;

        Vector2 startPosition = rb.position;
        Vector2 dashTarget = startPosition + direction * dashRange;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            // Lerp (Linear Interpolation) smoothly moves the player from the start to the target.
            Vector2 newPosition = Vector2.Lerp(startPosition, dashTarget, elapsedTime / dashDuration);
            rb.MovePosition(newPosition);

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate(); // Wait for the next physics update.
        }

        // Ensure the player ends exactly at the target position.
        rb.MovePosition(dashTarget);

        isInvincible = false;
        isDashing = false;
    }

    /// Temporarily changes the sprite color to indicate the dash is available.
    private IEnumerator DashReadyFlashCoroutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = dashReadyFlashColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
    }

    /// Updates the animator parameters to reflect the player's movement direction.
    private void SetAnimatorMovement(Vector2 direction)
    {
        animator.SetLayerWeight(1, 1);
        animator.SetFloat("xDir", direction.x);
        animator.SetFloat("yDir", direction.y);
    }

    /// Grants temporary I-frames and automatically resets the boolean when finished.
    public void SetTemporaryInvincibility(float duration)
    {
        StartCoroutine(InvincibilityTimerCoroutine(duration));
    }

    private IEnumerator InvincibilityTimerCoroutine(float duration)
    {
        isInvincible = true;

        yield return new WaitForSeconds(duration);

        // Ensure the player becomes vulnerable again after the duration expires
        isInvincible = false;
    }
}