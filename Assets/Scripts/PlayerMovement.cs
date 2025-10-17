using UnityEngine;
using System.Collections;

/// Handles player movement, input, and the dash ability.
/// This script requires a Rigidbody2D and an Animator component on the same GameObject.
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
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

    [Header("UI References")]
    [Tooltip("Reference to the DashCooldown script on the UI icon.")]
    public DashCooldown dashIconCooldown;

    // --- Private Variables ---
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 direction;
    private bool isDashing;
    private float lastDashTime;

    // Public property to check invincibility status from other scripts.
    // The setter is now public to allow other scripts (like PlayerStats) to modify it.
    public bool isInvincible { get; set; }

    private void Awake()
    {
        // Get component references once at the start.
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        // Initialize lastDashTime to allow dashing from the start.
        lastDashTime = -dashCooldown;
    }

    private void Update()
    {
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
        if (Input.GetKeyDown(KeyCode.Space) && direction != Vector2.zero && Time.time >= lastDashTime + dashCooldown)
        {
            PerformDash();
        }

        // Check for pause input.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // FindObjectOfType is slow, but acceptable for a pause menu.
            FindFirstObjectByType<SceneTransition>().LoadSceneWithFade("PauseScreen");
        }
    }

    /// Executes the dash ability and triggers the UI cooldown.
    private void PerformDash()
    {
        lastDashTime = Time.time;

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

    /// Updates the animator parameters to reflect the player's movement direction.
    private void SetAnimatorMovement(Vector2 direction)
    {
        animator.SetLayerWeight(1, 1);
        animator.SetFloat("xDir", direction.x);
        animator.SetFloat("yDir", direction.y);
    }
}

