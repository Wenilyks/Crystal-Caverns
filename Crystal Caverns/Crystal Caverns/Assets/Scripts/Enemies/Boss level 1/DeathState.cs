using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DeathState : BossState
{
    private float deathDuration = 2f;
    private float deathTimer = 0f;

    public DeathState(BossController boss) : base(boss) { }

    public override void Enter()
    {
        boss.animator.SetInteger("state", 6); 
        boss.rb.linearVelocity = Vector2.zero;
        deathTimer = 0f;
    }

    public override void Update()
    {
        deathTimer += Time.deltaTime;

        if (deathTimer >= deathDuration)
        {
            GameObject.Destroy(boss.gameObject);
        }
    }

    public override void Exit() { }
}