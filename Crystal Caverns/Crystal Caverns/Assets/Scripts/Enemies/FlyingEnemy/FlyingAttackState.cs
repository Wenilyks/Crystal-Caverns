using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class FlyingAttackState : FlyingEnemyState
{
    private float attackTimer = 0f;
    private bool isAttacking = false;
    private bool hasDealtDamage = false;
    public FlyingAttackState(FlyingEnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.Stop();
        isAttacking = false;
        hasDealtDamage = false;
        attackTimer = 0f;
    }

    public override void Update()
    {
        if (!enemy.IsPlayerInAttackRange())
        {
            if (enemy.CanSeePlayer())
            {
                enemy.ChangeState(enemy.chaseState);
            }
            else
            {
                enemy.ChangeState(enemy.patrolState);
            }
            return;
        }

        if (!isAttacking && Time.time >= enemy.lastAttackTime + enemy.attackCooldown)
        {
            StartAttack();
        }

        if (isAttacking)
        {
            attackTimer += Time.deltaTime;

            Vector2 playerPosition = enemy.playerTransform.position;
            Vector2 attackPosition = new Vector2(playerPosition.x, playerPosition.y + 1.5f);
            enemy.FlyTowards(attackPosition, enemy.patrolSpeed);

            if (!hasDealtDamage && attackTimer >= 0.3f)
            {
                // deal damage
                DealDamage();
                hasDealtDamage = true;
            }

            if (attackTimer >= 0.5f)
            {
                EndAttack();
            }
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = 0f;
        enemy.animator.SetInteger("state", 3);
        Debug.Log("Flying enemy is attacking");
    }

    private void DealDamage()
    {
        if (enemy.IsPlayerInAttackRange())
        {
            enemy.player.TakeDamage(enemy.attackDamage);
            Debug.Log("Flying enemy is dealing damage");
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
        enemy.lastAttackTime = Time.time;
        enemy.animator.SetInteger("state", 0);
    }

    public override void Exit()
    {
        isAttacking = false;
    }

    public override void OnDrawGizmos()
    {
        if (isAttacking)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(enemy.transform.position, enemy.attackRange);
        }
    }
}