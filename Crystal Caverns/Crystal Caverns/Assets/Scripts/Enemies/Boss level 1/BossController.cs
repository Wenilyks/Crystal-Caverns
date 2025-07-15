using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossController : MonoBehaviour, IDamageable
{
    [Header("Boss Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float moveSpeed = 2f;
    public float attackRange = 10f;
    public float attackCooldown = 2f;

    [Header("References")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator animator;

    [Header("Audio")]
    public string hurtSoundName = "Boss_Hurt";
    public string attackSoundName = "Boss_Attack";

    public float lastAttackTime;
    public bool isAttacking = false;
    protected bool facingRight = true;
    protected float aggressionMultiplier = 1f;
    protected int consecutiveAttacks = 0;

    protected BossState currentState;
    protected Dictionary<string, BossState> states = new Dictionary<string, BossState>();
    protected List<AttackBehaviour> attacks = new List<AttackBehaviour>();

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;
    public event Action<AttackBehaviour> OnAttackExecuted;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        InitializeStates();
        InitializeAttacks();

        ChangeState("Idle");
        lastAttackTime = Time.time;
    }

    protected virtual void Update()
    {
        if (currentHealth <= 0)
        {
            if (currentState?.GetType().Name != "DeathState")
                ChangeState("Death");
            return;
        }

        UpdateBossLogic();
        currentState?.Update();
    }

    protected abstract void InitializeStates();
    protected abstract void InitializeAttacks();
    protected abstract void UpdateBossLogic();

    public virtual void ChangeState(string stateName)
    {
        if (isAttacking && stateName != "Hurt" && stateName != "Death") return;

        if (states.ContainsKey(stateName))
        {
            currentState?.Exit();
            currentState = states[stateName];
            currentState?.Enter();
        }
    }

    public virtual void SelectExecuteAttack()
    {
        float highestPriority = 0f;
        AttackBehaviour bestAttack = null;

        foreach (var attack in attacks)
        {
            if (attack.CanExecute())
            {
                float priority = attack.GetPriority() * aggressionMultiplier;
                priority = ModifyAttackPriority(attack, priority);

                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    bestAttack = attack;
                }
            }
        }

        if (bestAttack != null)
        {
            bestAttack.Execute();
            OnAttackExecuted?.Invoke(bestAttack);
            consecutiveAttacks++;

            if (consecutiveAttacks > 1)
            {
                lastAttackTime = Time.time - (attackCooldown * 0.3f);
            }
        }
    }

    protected virtual float ModifyAttackPriority(AttackBehaviour attack, float basePriority)
    {
        return basePriority;
    }

    public virtual void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHealthChanged?.Invoke(currentHealth);
        consecutiveAttacks = 0;
        aggressionMultiplier = Mathf.Min(aggressionMultiplier * 1.3f, 2f);

        if (currentHealth > 0)
        {
            ChangeState("Hurt");
        }
        else
        {
            OnDeath?.Invoke();
        }

        Debug.Log($"Boss took {damage} damage. Health: {currentHealth}");
    }

    public virtual void HandleFlip(float directionX)
    {
        if (directionX > 0 && !facingRight)
            Flip();
        else if (directionX < 0 && facingRight)
            Flip();
    }

    protected virtual void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    public float GetDistanceToPlayer()
    {
        return player ? Vector2.Distance(transform.position, player.position) : float.MaxValue;
    }

    public Vector2 GetDirectionToPlayer()
    {
        return player ? (player.position - transform.position).normalized : Vector2.zero;
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}