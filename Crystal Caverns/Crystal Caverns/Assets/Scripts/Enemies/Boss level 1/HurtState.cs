using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HurtState : BossState
{
    private float hurtDuration = 0.5f;
    public float hurtTimer;
    public HurtState(BossController boss) : base(boss) { }

    public override void Enter()
    {
        boss.animator.SetInteger("state", 4);
        hurtTimer = 0f;
        boss.rb.linearVelocity = Vector2.zero;
    }

    public override void Update()
    {
        hurtTimer += Time.deltaTime;

        if (hurtTimer >= hurtDuration)
        {
            boss.ChangeState("Chasing");
        }
    }

    public override void Exit()
    {
        
    }
}
