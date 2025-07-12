using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class ChasingState : BossState
{
    public ChasingState(FireWizardBossController boss) : base(boss) { }

    public override void Enter()
    {
        boss.animator.SetInteger("state", 1);
    }

    public override void Update()
    {
        float distanceToPlayer = Vector2.Distance(boss.transform.position, boss.player.position);

        Vector2 direction = (boss.player.position - boss.transform.position).normalized;
        boss.rb.linearVelocity = new Vector2(direction.x * boss.moveSpeed, boss.rb.linearVelocityY);

        boss.HandleFlip(direction.x);

        if (Time.time - boss.lastAttackTime >= boss.attackCooldown && !boss.isAttacking)
        {
            boss.SelecteExecuteAttack();
        }


        if (distanceToPlayer > boss.attackRange * 1.5f)
        {
            boss.ChangeState(boss.idleState);
        }
    }

    public override void Exit()
    {
        boss.rb.linearVelocity = Vector2.zero;
    }

}
