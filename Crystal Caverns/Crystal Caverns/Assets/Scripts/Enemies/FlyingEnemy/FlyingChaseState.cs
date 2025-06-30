using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FlyingChaseState : FlyingEnemyState
{
    private float lostPlayerTimer = 0f;
    private float lostPlayerTime = 3f;

    public FlyingChaseState(FlyingEnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        lostPlayerTimer = 0f;
        enemy.animator.SetInteger("state", 0);
    }

    public override void Update()
    {
        if (enemy.player == null) return;
        if (enemy.IsPlayerInAttackRange())
        {
            // change state to attack
            enemy.ChangeState(enemy.attackState);
            return;
        }

        if (enemy.CanSeePlayer())
        {
            Vector2 playerPosition = enemy.player.position;

            Vector2 targetPosition = new Vector2(playerPosition.x, playerPosition.y + 1f);
            enemy.FlyTowards(targetPosition, enemy.chaseSpeed);
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

            enemy.FlyTowards(enemy.player.position, enemy.chaseSpeed);
        }
    }

    public override void Exit()
    {
        
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
