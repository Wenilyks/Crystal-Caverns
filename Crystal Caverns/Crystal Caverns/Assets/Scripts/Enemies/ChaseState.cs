using UnityEngine;

public class ChaseState : EnemyState
{
    private float lostPlayerTimer = 0f;
    private float lostPlayerTime = 2f;

    public ChaseState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        lostPlayerTimer = 0f;
        enemy.animator.SetInteger("state", 1);
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
            Debug.Log("I can see the player and starting to move");
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
