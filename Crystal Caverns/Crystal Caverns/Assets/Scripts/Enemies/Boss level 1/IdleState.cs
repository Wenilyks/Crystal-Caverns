using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IdleState : BossState
{
    public IdleState(BossController boss) : base(boss) { }

    public override void Enter()
    {
        boss.animator.SetInteger("state", 0);
        boss.rb.linearVelocity = Vector2.zero;

    }

    public override void Update()
    {
        float distanceToPlayer = boss.GetDistanceToPlayer();

        if (distanceToPlayer <= boss.attackRange)
        {
            boss.ChangeState(BossController.STATE_CHASING);
        }
    }

    public override void Exit()
    {
    }
}