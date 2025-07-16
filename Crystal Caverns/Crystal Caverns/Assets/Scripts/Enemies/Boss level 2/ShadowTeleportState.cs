using UnityEngine;

public class ShadowTeleportState : BossState
{
    private float teleportDuration = 1f;
    private float teleportTimer = 0f;

    public ShadowTeleportState(BossController boss) : base(boss) { }

    public override void Enter()
    {
        boss.animator.SetInteger("state", 5);
        boss.rb.linearVelocity = Vector2.zero;
        teleportTimer = 0f;
    }

    public override void Update()
    {
        teleportTimer += Time.deltaTime;

        if (teleportTimer >= teleportDuration)
        {
            boss.ChangeState("Chasing");
        }
    }

    public override void Exit()
    {
    }
}