using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class DeathState : BossState
{
    [Header("Death State Settings")]
    public float deathDuration = 2f;
    private float deathTimer = 0f;

    public DeathState(BossController boss) : base(boss) { }

    public override void Enter()
    {
        boss.rb.linearVelocity = Vector2.zero;
        deathTimer = 0f;

        boss.InterruptAttack();

        if (!string.IsNullOrEmpty(boss.hurtSoundName))
        {
            AudioManager.Instance?.PlaySFX(boss.hurtSoundName);
        }
        boss.animator.SetInteger("state", 10);
    }

    public override void Update()
    {
        deathTimer += Time.deltaTime;

        if (deathTimer >= deathDuration)
        {
            HandleDeathEffects();

            GameObject.Destroy(boss.gameObject);
        }
    }

    private void HandleDeathEffects()
    {
        boss.DestroySelf();
    }

    public override void Exit()
    {
    }
}