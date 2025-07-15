using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class ChasingState : BossState
{
    private float lostPlayerTimer = 0f;
    private float lostPlayerTime = 2f;
    private float lastPlayerDistance = 0f;
    private bool playerWasInRange = false;
    public ChasingState(BossController boss) : base(boss) { }

    public override void Enter()
    {
        boss.animator.SetInteger("state", 1);
        lostPlayerTimer = 0f;
        playerWasInRange = false;
    }

    public override void Update()
    {
        float distanceToPlayer = Vector2.Distance(boss.transform.position, boss.player.position);

        if (distanceToPlayer <= boss.attackRange)
        {
            playerWasInRange = true;
            lostPlayerTimer = 0f;
        }
        else
        {
            lostPlayerTimer += Time.deltaTime;
        }

        Vector2 direction = (boss.player.position - boss.transform.position).normalized;
        float dynamicMoveSpeed = boss.moveSpeed;

        if (distanceToPlayer > lastPlayerDistance && distanceToPlayer < boss.attackRange)
        {
            dynamicMoveSpeed *= 1.5f;
        }

        if (playerWasInRange && lostPlayerTimer < lostPlayerTime)
        {
            dynamicMoveSpeed *= 1.3f;
        }

        boss.rb.linearVelocity = new Vector2(direction.x * dynamicMoveSpeed, boss.rb.linearVelocityY);
        boss.HandleFlip(direction.x);

        bool shouldAttack = false;

        if (distanceToPlayer <= boss.attackRange && !playerWasInRange)
        {
            shouldAttack = true;
        }
        else if (distanceToPlayer <= boss.attackRange &&
                 Time.time - boss.lastAttackTime >= boss.attackCooldown)
        {
            shouldAttack = true;
        }
        else if (distanceToPlayer <= ((FireWizardBossController)boss).fireHandsRange &&
                 Time.time - boss.lastAttackTime >= boss.attackCooldown * 0.6f)
        {
            shouldAttack = true;
        }

        if (shouldAttack && !boss.isAttacking)
        {
            boss.SelectExecuteAttack();
        }

        float extendedRange = boss.attackRange * 2.5f;
        if (distanceToPlayer > extendedRange && lostPlayerTimer > lostPlayerTime)
        {
            boss.ChangeState("Idle");
        }
    }

    public override void Exit()
    {
        boss.rb.linearVelocity = Vector2.zero;
        playerWasInRange = false;
    }

}
