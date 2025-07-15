using UnityEngine;
using UnityEngine.Rendering;

public class FireWizardBossController : BossController
{
    [Header("Fire Wizard Specific")]
    public float fireHandsRange = 2f;
    public float fireHandsDamage = 20f;
    public float fireballDamage = 15f;
    public float fireballRainDamage = 10f;
    public GameObject fireballPrefab;
    public Transform firePoint;
    public GameObject fireballRainPrefab;

    protected override void InitializeStates()
    {
        states["Idle"] = new IdleState(this);
        states["Chasing"] = new ChasingState(this);
        states["Hurt"] = new HurtState(this);
        states["Death"] = new DeathState(this);
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
    }

    protected override float ModifyAttackPriority(AttackBehaviour attack, float basePriority)
    {
        if (attack is FireHandsAttack && GetDistanceToPlayer() <= fireHandsRange)
            return basePriority * 1.5f;

        if (attack is FireballAttack && GetDistanceToPlayer() > fireHandsRange)
            return basePriority * 1.2f;

        return basePriority;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fireHandsRange);
    }
}