using UnityEngine;

public class BaelorisMovement : MonoBehaviour
{
    public Transform player;
    public float radius = 10f;
    public float speed = 3f;
    public float timeBetweenMoves = 5f;

    private Vector3 targetPosition;
    private float moveTimer;
    private bool isCombatActive = false;

    private void Update()
    {
        if (!isCombatActive || player == null) return;

        EnemyReceiveDamage damageScript = GetComponent<EnemyReceiveDamage>();

        if (damageScript.health > damageScript.maxHealth / 2)
        {
            moveTimer -= Time.deltaTime;

            if (moveTimer <= 0)
            {
                PickNewTargetPosition();
            }

            MoveTowardsTarget();
        }
        else
        {
            transform.position = new Vector3(0, 0, 0);
        }
    }

    public void BeginMovementPhase()
    {
        isCombatActive = true;
    }

    public void StopMovementPhase()
    {
        isCombatActive = false;
    }

    public void PickNewTargetPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        targetPosition = new Vector3(player.position.x + randomCircle.x, player.position.y, 0);
        moveTimer = timeBetweenMoves;
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }
}