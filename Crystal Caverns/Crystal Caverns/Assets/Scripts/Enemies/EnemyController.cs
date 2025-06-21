using System;
using UnityEngine;
using Unity;


public class EnemyController : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public LayerMask playerLayer = 1;
    public LayerMask obstacleLayer = 1;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public Transform[] patrolPoints;

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f;

    [Header("Components")]
    public Rigidbody2D rb { get; private set; }
    public Transform player { get; private set; }
    public Animator animator { get; private set; }

    [Header("State Machine")]
    public EnemyState currentState { get; private set; }
    public PatrolState patrolState { get; private set; }
    public ChaseState chaseState { get; private set; }
    public AttackState attackState { get; private set; }

    public int currentPatrolIndex = 0;
    public float lastAttackTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        patrolState = new PatrolState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Start()
    {
        ChangeState(patrolState);
        animator.SetInteger("state", 1);
    }

    private void Update()
    {
        currentState?.Update();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public bool CanSeePlayer()
    {
        if (player == null) Debug.Log("NOOO PLAYER IS NULLL");
        if (player == null) return false;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer > detectionRange) return false;

        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, detectionRange, obstacleLayer);

        return hit.collider == null;
    }

    public bool IsPlayerInAttackRange()
    {
        if (player == null) return false;

        return Vector2.Distance(transform.position, player.position) <= attackRange;
    }

    public void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocityY);

        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    public void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocityY);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        currentState?.OnDrawGizmos();
    }
}
