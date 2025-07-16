using UnityEngine;

public class ShadowBossController : BossController
{
    [Header("Shadow Boss Specific")]
    public float shadowStrikeRange = 3f;
    public float shadowStrikeDamage = 25f;
    public float shadowBoltDamage = 18f;
    public float shadowWaveDamage = 12f;
    public float teleportRange = 8f;
    public float teleportCooldown = 5f;
    public GameObject shadowBoltPrefab;
    public GameObject shadowWavePrefab;
    public Transform[] shadowBoltSpawnPoints;
    public ParticleSystem teleportEffect;

    [Header("Shadow Boss Behavior")]
    public float shadowStrikeCooldownMultiplier = 0.4f;
    public float lowHealthAggressionThreshold = 0.3f;
    public float lowHealthAggressionMultiplier = 2.5f;

    private float lastTeleportTime;
    private bool canTeleport = true;

    protected override void Start()
    {
        base.Start();
        lastTeleportTime = Time.time;
    }

    protected override void InitializeStates()
    {
        states[STATE_IDLE] = new IdleState(this);
        states[STATE_CHASING] = new ChasingState(this);
        states[STATE_HURT] = new HurtState(this);
        states[STATE_DEATH] = new DeathState(this);
        states["Teleporting"] = new ShadowTeleportState(this);
    }

    protected override void InitializeAttacks()
    {
        attacks.Add(new ShadowStrikeAttack(this));
        attacks.Add(new ShadowBoltAttack(this));
        attacks.Add(new ShadowWaveAttack(this));
        attacks.Add(new ShadowTeleportAttack(this));
    }

    protected override void UpdateBossLogic()
    {
        float currentDistance = GetDistanceToPlayer();

        canTeleport = Time.time - lastTeleportTime >= teleportCooldown;

        if (currentHealth / maxHealth < lowHealthAggressionThreshold)
        {
            aggressionMultiplier = lowHealthAggressionMultiplier;
        }
    }

    public override float GetSpecialAttackRange()
    {
        return shadowStrikeRange;
    }

    public override float GetModifiedAttackCooldown()
    {
        return attackCooldown * shadowStrikeCooldownMultiplier;
    }

    protected override float ModifyAttackPriority(AttackBehaviour attack, float basePriority)
    {
        float distance = GetDistanceToPlayer();

        if (attack is ShadowStrikeAttack && distance <= shadowStrikeRange)
            return basePriority * 1.8f;

        if (attack is ShadowTeleportAttack && distance > teleportRange && canTeleport)
            return basePriority * 1.5f;

        if (attack is ShadowBoltAttack && distance > shadowStrikeRange && distance <= attackRange)
            return basePriority * 1.3f;

        if (attack is ShadowWaveAttack && currentHealth / maxHealth < 0.5f)
            return basePriority * 1.4f;

        return basePriority;
    }

    public void SetTeleportCooldown()
    {
        lastTeleportTime = Time.time;
        canTeleport = false;
    }

    public bool CanTeleport()
    {
        return canTeleport;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Shadow strike range
        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(transform.position, shadowStrikeRange);

        // Teleport range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, teleportRange);
    }
}