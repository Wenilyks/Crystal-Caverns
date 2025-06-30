using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttackState : EnemyState
{
    private float attackTimer = 0f;
    private bool isAttacking = false;
    private bool hasDealtDamage = false;

    public AttackState(EnemyController enemy) : base(enemy) { }

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
                enemy.ChangeState(enemy.chaseState);
            else
                enemy.ChangeState(enemy.patrolState);
            return;
        }

        if (!isAttacking && Time.time >= enemy.lastAttackTime + enemy.attackCooldown)
        {
            StartAttack();
        }

        if (isAttacking)
        {
            attackTimer += Time.deltaTime;

            if (!hasDealtDamage && attackTimer >= 0.25f)
            {
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
        Debug.Log("Attacking lol");
    }

    private void DealDamage()
    {
        if (enemy.IsPlayerInAttackRange())
        {
            enemy.player.TakeDamage(enemy.attackDamage);
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
