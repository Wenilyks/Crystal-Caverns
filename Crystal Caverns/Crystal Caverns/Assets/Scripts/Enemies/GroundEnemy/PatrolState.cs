using System;
using UnityEngine;


public class PatrolState : EnemyState
{
    private float waitTimer = 0f;
    private float waitTime = 1f;

    public PatrolState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        waitTimer = 0f;
        enemy.animator.SetInteger("state", 1);
    }

    public override void Update()
    {
        if (enemy.CanSeePlayer())
        {
            Debug.Log("Start to chase player");
            enemy.ChangeState(enemy.chaseState);
            return;
        }

        if (enemy.patrolPoints.Length == 0) return;

        Transform targetPoint  = enemy.patrolPoints[enemy.currentPatrolIndex];
        float distToTarget = Vector2.Distance(enemy.transform.position, targetPoint.position);

        if (distToTarget > 0.3f)
        {
            enemy.MoveTowards(targetPoint.position, enemy.patrolSpeed);
            waitTimer = 0f;
        }
        else
        {
            enemy.Stop();
            waitTimer += Time.deltaTime;
            enemy.animator.SetInteger("state", 0);

            if (waitTimer >= waitTime)
            {
                enemy.currentPatrolIndex = (enemy.currentPatrolIndex + 1) % enemy.patrolPoints.Length;
                waitTimer = 0f;
                enemy.animator.SetInteger("state", 1);
            }
        }
    }

    public override void Exit()
    {
        enemy.Stop();
    }

    public override void OnDrawGizmos()
    {
        if (enemy.patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            Transform target = enemy.patrolPoints[enemy.currentPatrolIndex];
            Gizmos.DrawLine(enemy.transform.position, target.position); 
        }
    }
}

