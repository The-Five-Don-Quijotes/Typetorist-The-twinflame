using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float dashRange;
    public float dashCooldown = 2f;
    private float lastDashTime;
    public float speed;
    public float dashDuration = 0.2f; 
    private Vector2 direction;
    private Animator animator;
    private bool isDashing;
    private Collider2D playerCollider;
    public bool isInvincible = false;
    private Rigidbody2D rb;


    private void Start()
    {
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isDashing) 
        {
            TakeInput();
        }
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            Move();
        }
    }

    private void Move()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        if (direction.magnitude > 0)
        {
            SetAnimatorMovement(direction);
        }
        else
        {
            animator.SetLayerWeight(1, 0);
        }
    }

    private void TakeInput()
    {
        direction = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow))
            direction += Vector2.up;
        if (Input.GetKey(KeyCode.LeftArrow))
            direction += Vector2.left;
        if (Input.GetKey(KeyCode.DownArrow))
            direction += Vector2.down;
        if (Input.GetKey(KeyCode.RightArrow))
            direction += Vector2.right;

        if (direction.magnitude > 1)
            direction = direction.normalized;

        // Handle Dash
        if (Input.GetKeyDown(KeyCode.Space) && direction != Vector2.zero && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            lastDashTime = Time.time;
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        isInvincible = true;

        Vector2 dashTarget = (Vector2)transform.position + direction * dashRange;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            Vector2 newPosition = Vector2.Lerp(rb.position, dashTarget, elapsedTime / dashDuration);
            rb.MovePosition(newPosition); 

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate(); 
        }

        rb.MovePosition(dashTarget);

        isInvincible = false;
        isDashing = false;
    }


    private void SetAnimatorMovement(Vector2 direction)
    {
        animator.SetLayerWeight(1, 1);
        animator.SetFloat("xDir", direction.x);
        animator.SetFloat("yDir", direction.y);
    }
}
