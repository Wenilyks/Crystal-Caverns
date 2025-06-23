using UnityEngine;

public class ChaseState : EnemyState
{
    private float lostPlayerTimer = 0f;
    private float lostPlayerTime = 2f;
    private float stuckTimer = 0f;
    private float stuckCheckTime = 0.6f;
    private float finalStuckCheckTime = 0.7f;
    private Vector2 lastPosition;

    public ChaseState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        lostPlayerTimer = 0f;
        stuckTimer = 0f;
        enemy.animator.SetInteger("state", 1);
        lastPosition = enemy.transform.position;
    }

    public override void Update()
    {
        if (enemy.IsPlayerInAttackRange())
        {
            enemy.ChangeState(enemy.attackState);
            return;
        }

        Debug.Log("Calling can see player function");
        if (enemy.CanSeePlayer())
        {
            enemy.MoveTowards(enemy.player.position, enemy.chaseSpeed);
            lostPlayerTimer = 0f;
        }
        else
        {
            lostPlayerTimer += Time.deltaTime;  

            if (lostPlayerTimer >= lostPlayerTime)
            {
                enemy.ChangeState(enemy.patrolState);
                return;
            }

            enemy.MoveTowards(enemy.player.position, enemy.chaseSpeed);
        }

        CheckIfStuck();
    }
    private void CheckIfStuck()
    {
        float distanceMoved = Vector2.Distance(enemy.transform.position, lastPosition);

        if (distanceMoved < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            
            if (stuckTimer >= stuckCheckTime && enemy.IsGrounded())
            {
                Vector2 directionToPlayer = (enemy.player.position - enemy.transform.position).normalized;
                enemy.Jump(directionToPlayer);
            }

            if (stuckTimer >= finalStuckCheckTime) 
            {
                if (enemy.patrolPoints != null && enemy.patrolPoints.Length > 0)
                {
                    enemy.MoveTowards(new Vector2(enemy.patrolPoints[0].position.x, enemy.patrolPoints[0].position.y), enemy.patrolSpeed);
                }
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = enemy.transform.position;
    }

    public override void Exit()
    {
        enemy.Stop();
    }

    public override void OnDrawGizmos()
    {
        if (enemy.player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(enemy.transform.position, enemy.player.position);
        }
    }
    
}
