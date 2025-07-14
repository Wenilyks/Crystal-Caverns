using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class FireWizardBossController : MonoBehaviour, IDamageable
{
    [Header("Boss stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float moveSpeed = 2f;
    public float attackRange = 10f;
    public float fireHandsRange = 2f;

    [Header("Attack settings")]
    public float attackCooldown = 2f;
    public float fireHandsDamage = 20f;
    public float fireballDamage = 15f;
    public float fireballRainDamage = 10f;

    [Header("References")]
    public Transform player;
    public GameObject fireballPrefab;
    public Transform firePoint;
    public GameObject fireballRainPrefab;

    public Rigidbody2D rb;
    public Animator animator;
    public float lastAttackTime;
    public bool isAttacking = false;

    private float lastPlayerDistance = 0f;
    private bool playerMovingAway = false;
    private float aggressionMultiplier = 1f;
    private int consecutiveAttacks = 0;
    private float lastAttackCheckTime = 0f;

    // boss state
    private BossState currentState;
    public IdleState idleState;
    public ChasingState chasingState;
    public HurtState hurtState;

    private bool facingRight = true;
    private List<AttackBehaviour> attacks = new List<AttackBehaviour>();
    // attack behaviour

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // initialize attack behaviours
        attacks = new List<AttackBehaviour>
        {
            new FireballAttack(this),
            new FireHandsAttack(this),
            new FireballRainAttack(this),
        };

        // state to the idle
        idleState = new IdleState(this);
        chasingState = new ChasingState(this);
        hurtState = new HurtState(this);

        ChangeState(idleState);

        lastAttackTime = Time.time;
    }

    private void Update()
    {
        if (currentHealth <= 0)
        {
            // changing state to the death state
            return;
        }

        UpdatePlayerTracking();

        // update current state
        currentState?.Update();
    }

    private void UpdatePlayerTracking()
    {
        float currentDistance = Vector2.Distance(transform.position, player.transform.position);

        playerMovingAway = currentDistance > lastPlayerDistance;

        // increasing aggression when player is kiting
        if (playerMovingAway && currentDistance < attackRange * 1.5f)
        {
            aggressionMultiplier = Mathf.Min(aggressionMultiplier + Time.deltaTime * 0.5f, 2f);
        }
        else
        {
            aggressionMultiplier = Mathf.Max(aggressionMultiplier - Time.deltaTime * 0.2f, 1f);
        }
        lastPlayerDistance = currentDistance;
    }

    public void ChangeState(BossState state) 
    {
        if (isAttacking && !(state is HurtState)) return;
        // exit current state
        currentState?.Exit();
        // assign new state
        currentState = state;
        // enter current state
        currentState?.Enter();
    }

    public void SelectExecuteAttack()
    {
        float highestPriority = 0f;
        AttackBehaviour bestAttack = null;

        foreach (var attack in attacks)
        {
            if (attack.CanExecute())
            {
                float priority = attack.GetPriority() * aggressionMultiplier;

                if (attack is FireHandsAttack && playerMovingAway) priority *= 1.5f;

                if (attack is FireballAttack && Vector2.Distance(transform.position, player.position) > fireHandsRange) priority *= 1.2f;

                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    bestAttack = attack;
                }
            }
        }

        // execute best attack
        if (bestAttack != null)
        {
            bestAttack.Execute();
            consecutiveAttacks++;

            if (consecutiveAttacks > 1)
            {
                lastAttackTime = Time.time - (attackCooldown * 0.3f);
            }
        }
    }

    public float GetDynamicAttackCooldown()
    {
        float baseCooldown = attackCooldown;

        baseCooldown /= aggressionMultiplier;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= fireHandsRange)
            baseCooldown *= 0.7f;

        float healthPercentage = currentHealth / maxHealth;
        if (healthPercentage < 0.5f)
        {
            baseCooldown *= 0.8f;
        }

        return Mathf.Max(baseCooldown, 0.5f);
    }

    public void HandleFlip(float directionX)
    {
        if (directionX > 0 && !facingRight)
            Flip();
        else if (directionX < 0 && facingRight)
            Flip();
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        consecutiveAttacks = 0;

        aggressionMultiplier = Mathf.Min(aggressionMultiplier * 0.3f, 2f);

        if (currentHealth > 0)
        {
            ChangeState(hurtState);
        }

        Debug.Log($"Boss took damage: {damage} damage. Health: {currentHealth}");
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fireHandsRange);
    }
}