using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChasingState : BossState
{
    private float lostPlayerTimer = 0f;
    private float lastPlayerDistance = 0f;
    private bool playerWasInRange = false;

    private const float CATCHING_UP_SPEED_MULTIPLIER = 1.5f;
    private const float PURSUIT_SPEED_MULTIPLIER = 1.3f;
    private const float SPECIAL_ATTACK_COOLDOWN_MULTIPLIER = 0.6f;

    public ChasingState(BossController boss) : base(boss) { }

    public override void Enter()
    {
        boss.animator.SetInteger("state", 1);
        lostPlayerTimer = 0f;
        playerWasInRange = false;
    }

    public override void Update()
    {
        float distanceToPlayer = boss.GetDistanceToPlayer();

        if (distanceToPlayer <= boss.attackRange)
        {
            playerWasInRange = true;
            lostPlayerTimer = 0f;
        }
        else
        {
            lostPlayerTimer += Time.deltaTime;
        }

        Vector2 direction = boss.GetDirectionToPlayer();
        float dynamicMoveSpeed = CalculateDynamicMoveSpeed(distanceToPlayer);

        boss.rb.linearVelocity = new Vector2(direction.x * dynamicMoveSpeed, boss.rb.linearVelocity.y);
        boss.HandleFlip(direction.x);

        if (ShouldAttack(distanceToPlayer) && !boss.isAttacking)
        {
            boss.SelectExecuteAttack();
        }

        if (ShouldReturnToIdle(distanceToPlayer))
        {
            boss.ChangeState(BossController.STATE_IDLE);
        }
    }

    private float CalculateDynamicMoveSpeed(float distanceToPlayer)
    {
        float dynamicMoveSpeed = boss.moveSpeed;

        if (distanceToPlayer > lastPlayerDistance && distanceToPlayer < boss.attackRange)
        {
            dynamicMoveSpeed *= CATCHING_UP_SPEED_MULTIPLIER;
        }

        if (playerWasInRange && lostPlayerTimer < boss.lostPlayerTime)
        {
            dynamicMoveSpeed *= PURSUIT_SPEED_MULTIPLIER;
        }

        lastPlayerDistance = distanceToPlayer;
        return dynamicMoveSpeed;
    }

    private bool ShouldAttack(float distanceToPlayer)
    {
        if (distanceToPlayer <= boss.attackRange && !playerWasInRange)
        {
            return true;
        }

        if (distanceToPlayer <= boss.attackRange &&
            Time.time - boss.lastAttackTime >= boss.attackCooldown)
        {
            return true;
        }

        if (distanceToPlayer <= boss.GetSpecialAttackRange() &&
            Time.time - boss.lastAttackTime >= boss.GetModifiedAttackCooldown() * SPECIAL_ATTACK_COOLDOWN_MULTIPLIER)
        {
            return true;
        }

        return false;
    }

    private bool ShouldReturnToIdle(float distanceToPlayer)
    {
        float extendedRange = boss.GetExtendedRange();
        return distanceToPlayer > extendedRange && lostPlayerTimer > boss.lostPlayerTime;
    }

    public override void Exit()
    {
        boss.rb.linearVelocity = Vector2.zero;
        playerWasInRange = false;
    }
}