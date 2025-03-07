using UnityEngine;

public class BaelorisMovement : MonoBehaviour
{
    public Transform player;
    public float radius = 10f;
    public float speed = 3f;
    public float timeBetweenMoves = 5f;

    private Vector3 targetPosition;
    private float moveTimer;

    void Start()
    {
    }

    void Update()
    {
        moveTimer -= Time.deltaTime;

        if (moveTimer <= 0)
        {
            PickNewTargetPosition();
        }

        MoveTowardsTarget();
        if(transform.position == targetPosition)
        {
            //GetComponent<Animator>().SetBool("isFlying", false);
        }
    }

    void PickNewTargetPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        targetPosition = new Vector3(player.position.x + randomCircle.x, player.position.y, 0);
        moveTimer = timeBetweenMoves;

        //GetComponent<Animator>().SetBool("isFlying", true);
    }

    void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }
}
