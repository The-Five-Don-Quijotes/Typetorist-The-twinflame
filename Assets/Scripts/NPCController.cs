using UnityEngine;

public class NPCController : MonoBehaviour
{
    private Animator animator;
    private Vector2 moveDirection;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Replace this vector with your actual NPC pathfinding or AI movement vector
        moveDirection = GetNPCMovementVector();

        // Update exact direction only when moving to preserve the last facing direction for Idle
        if (moveDirection != Vector2.zero)
        {
            animator.SetFloat("MoveX", moveDirection.x);
            animator.SetFloat("MoveY", moveDirection.y);
        }

        // Pass the squared magnitude to trigger Walk or Idle states
        animator.SetFloat("Speed", moveDirection.sqrMagnitude);
    }

    // Dummy method for context. Replace with your actual logic.
    private Vector2 GetNPCMovementVector()
    {
        // Example: Input mapping or AI NavMesh velocity
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        return new Vector2(moveX, moveY).normalized;
    }
}