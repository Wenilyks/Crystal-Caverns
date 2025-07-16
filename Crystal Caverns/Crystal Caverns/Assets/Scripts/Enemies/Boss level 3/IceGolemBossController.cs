using UnityEngine;

public class IceGolemBossController : BossController
{
    [Header("Ice Golem Specific")]
    public float punchRange = 3f;
    public float punchDamage = 25f;
    public float iceShardDamage = 12f;
    public GameObject iceShardPrefab;
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    [Header("Ice Golem Behavior")]
    public float punchCooldownMultiplier = 0.8f;
    public float slowDownEffect = 0.5f;
    public float slowDownDuration = 2f;

    [Header("Ice Golem Audio")]
    public string iceShardSoundName = "Ice_Shard";

    private float originalMoveSpeed;

    protected override void Start()
    {
        originalMoveSpeed = moveSpeed;
        base.Start();
    }

    protected override void InitializeStates()
    {
        states[STATE_IDLE] = new IdleState(this);
        states[STATE_CHASING] = new ChasingState(this);
        states[STATE_HURT] = new HurtState(this);
        states[STATE_DEATH] = new DeathState(this);
    }

    protected override void InitializeAttacks()
    {
        attacks.Add(new IcePunchAttack(this));
        attacks.Add(new IceShardBarrageAttack(this));
    }

    protected override void UpdateBossLogic()
    {
        float currentDistance = GetDistanceToPlayer();

        if (currentDistance <= punchRange)
        {
            aggressionMultiplier = Mathf.Min(aggressionMultiplier * 1.2f, maxAggressionMultiplier);
        }

        moveSpeed = originalMoveSpeed;

        if (currentDistance > attackRange * 1.5f)
        {
            moveSpeed = originalMoveSpeed * 0.8f;
        }
    }

    public override float GetSpecialAttackRange()
    {
        return punchRange;
    }

    public override float GetModifiedAttackCooldown()
    {
        return attackCooldown * punchCooldownMultiplier;
    }

    protected override float ModifyAttackPriority(AttackBehaviour attack, float basePriority)
    {
        float distance = GetDistanceToPlayer();

        if (attack is IcePunchAttack && distance <= punchRange)
            return basePriority * 1.8f;

        if (attack is IceShardBarrageAttack && distance > punchRange && distance <= attackRange)
            return basePriority * 1.3f;

        return basePriority;
    }

    public bool CanExecuteIceShardAttack()
    {
        return iceShardPrefab != null &&
               leftSpawnPoint != null &&
               rightSpawnPoint != null;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, punchRange);

        if (leftSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(leftSpawnPoint.position, 0.5f);
        }

        if (rightSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rightSpawnPoint.position, 0.5f);
        }
    }
}