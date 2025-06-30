using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HitState : EnemyState
{
    private float hitTimer = 0f;
    private float hitTime = 0.7f;
    private float hitAnimationTimer = 0f;
    private float hitAnimationTime = 0.3f;
    public HitState(EnemyController enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        hitTimer = 0f;
        enemy.animator.SetInteger("state", 4);
    }
    public override void Update() 
    {
        hitTimer += Time.deltaTime;
        if (hitTimer >= hitTime)
        {
            if (enemy.IsPlayerInAttackRange())
            {
                enemy.ChangeState(enemy.attackState);
            }
            else if (enemy.CanSeePlayer())
            {
                enemy.ChangeState(enemy.chaseState);
            }
            else
            {
                enemy.ChangeState(enemy.patrolState);
            }
        }

        hitAnimationTime += Time.deltaTime;
        if (hitAnimationTimer >= hitAnimationTime)
        {
            enemy.animator.SetInteger("state", 0);
        }
    }

    public override void Exit() 
    {
        hitAnimationTimer = 0f;
        hitTimer = 0f;
    }

    public override void OnDrawGizmos() { }
}