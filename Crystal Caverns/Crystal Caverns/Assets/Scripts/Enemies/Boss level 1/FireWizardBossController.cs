using UnityEngine;
using UnityEngine.Rendering;

public class FireWizardBossController : BossController
{
    [Header("Fire Wizard Specific")]
    public float fireHandsRange = 2f;
    public float fireHandsDamage = 30f;
    public float fireballDamage = 20f;
   
    public GameObject fireballPrefab;
    public Transform firePoint;
    public GameObject fireballRainPrefab;

    [Header("Fire Wizard Behavior")]
    public float fireHandsCooldownMultiplier = 0.6f;

    protected override void InitializeStates()
    {
        states[STATE_IDLE] = new IdleState(this);
        states[STATE_CHASING] = new ChasingState(this);
        states[STATE_HURT] = new HurtState(this);
        states[STATE_DEATH] = new DeathState(this);
    }

    protected override void InitializeAttacks()
    {
        attacks.Add(new FireballAttack(this));
        attacks.Add(new FireHandsAttack(this));
        attacks.Add(new FireballRainAttack(this));
    }

    protected override void UpdateBossLogic()
    {
        float currentDistance = GetDistanceToPlayer();

        if (currentDistance <= fireHandsRange)
        {
            aggressionMultiplier = Mathf.Min(aggressionMultiplier * 1.1f, maxAggressionMultiplier);
        }
    }

    public override float GetSpecialAttackRange()
    {
        return fireHandsRange;
    }

    public override float GetModifiedAttackCooldown()
    {
        return attackCooldown * fireHandsCooldownMultiplier;
    }

    protected override float ModifyAttackPriority(AttackBehaviour attack, float basePriority)
    {
        float distance = GetDistanceToPlayer();

        if (attack is FireHandsAttack && distance <= fireHandsRange)
            return basePriority * 1.5f;

        if (attack is FireballAttack && distance > fireHandsRange)
            return basePriority * 1.2f;

        return basePriority;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Draw fire hands range
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, fireHandsRange);
    }
}