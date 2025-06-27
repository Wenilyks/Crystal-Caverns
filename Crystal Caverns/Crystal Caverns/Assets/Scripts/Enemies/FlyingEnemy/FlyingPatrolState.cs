using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FlyingPatrolState : FlyingEnemyState
{
    private float waitTimer = 0f;
    private float waitTime = 2f;

    public FlyingPatrolState(FlyingEnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        waitTimer = 0f;
        enemy.animator.SetInteger("state", 0);
        enemy.SetBasePosition(enemy.transform.position);
    }

    public override void Update()
    {
        if (enemy.CanSeePlayer())
        {
            // change state to chase
            enemy.ChangeState(enemy.chaseState);
            return;
        }

        if (enemy.patrolPoints.Length == 0)
        {
            enemy.Hover();
            return;
        }

        Transform targetPoint = enemy.patrolPoints[enemy.currentPatrolIndex];
        Vector2 targetPosition = new Vector2(targetPoint.position.x,
            Mathf.Max(targetPoint.position.y, enemy.hoverHeight));

        float distToTraget = Vector2.Distance(enemy.transform.position, targetPosition);

        if (distToTraget > 0.5f)
        {
            enemy.FlyTowards(targetPosition, enemy.patrolSpeed);
            waitTimer = 0f;
        }

        else
        {
            enemy.SetBasePosition(targetPosition);
            enemy.Hover();
            waitTimer += Time.deltaTime;
            enemy.animator.SetInteger("state", 0);

            if (waitTimer >= waitTime)
            {
                enemy.currentPatrolIndex = (enemy.currentPatrolIndex + 1) % enemy.patrolPoints.Length;
                waitTimer = 0f;
                enemy.animator.SetInteger("state", 0);
            }
        }
    }

    public override void Exit()
    {
        
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

