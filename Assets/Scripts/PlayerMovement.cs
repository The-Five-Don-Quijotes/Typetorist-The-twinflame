using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float dashRange;
    public float speed;
    public float dashDuration = 0.2f; 
    private Vector2 direction;
    private Animator animator;
    private bool isDashing;
    private Collider2D playerCollider;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!isDashing) 
        {
            TakeInput();
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
        if (Input.GetKeyDown(KeyCode.Space) && direction != Vector2.zero && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        playerCollider.enabled = false; 
        animator.SetBool("isDashing", true); 

        Vector3 dashTarget = transform.position + (Vector3)(direction * dashRange);
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.Lerp(transform.position, dashTarget, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = dashTarget; 
        //animator.SetBool("isDashing", false); 
        playerCollider.enabled = true; 

        isDashing = false;
    }

    private void SetAnimatorMovement(Vector2 direction)
    {
        animator.SetLayerWeight(1, 1);
        animator.SetFloat("xDir", direction.x);
        animator.SetFloat("yDir", direction.y);
    }
}
