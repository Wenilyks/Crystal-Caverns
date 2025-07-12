using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IdleState : BossState
{
    public IdleState(FireWizardBossController boss) : base(boss) { }

    public override void Enter()
    {
        boss.animator.SetInteger("state", 0);
        boss.rb.linearVelocity = Vector2.zero;
    }

    public override void Update()
    {
        float distanceToPlayer = Vector2.Distance(boss.transform.position, boss.player.position);
        if (distanceToPlayer <= boss.attackRange)
        {
            boss.ChangeState(boss.chasingState);
        }
    }

    public override void Exit()
    {
        
    }
}
