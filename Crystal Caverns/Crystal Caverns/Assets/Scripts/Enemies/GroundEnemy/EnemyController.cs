using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 2.5f;
    public LayerMask playerLayer = 1;
    public LayerMask obstacleLayer = 1;
    public LayerMask groundLayer = 1;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public Transform[] patrolPoints;

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    public float targetHealth = 100f;

    [Header("UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private float barAnimationSpeed = 0.7f;

    [Header("Jumping")]
    public float jumpForce = 10f;
    public float jumpCooldown = 1f;
    public float obstacleCheckDistance = 1f;
    public float jumpCheckHeight = 2f;
    public float groundCheckDistance = 0.4f;

    [Header("Components")]
    public Rigidbody2D rb { get; private set; }
    public Transform playerTransform { get; private set; }
    public Hero2 player { get; private set; }
    public Animator animator { get; private set; }

    [Header("State Machine")]
    public EnemyState currentState { get; private set; }
    public PatrolState patrolState { get; private set; }
    public ChaseState chaseState { get; private set; }
    public AttackState attackState { get; private set; }
    public HitState hitState { get; private set; }

    public string currentStateString = "patrol";

    public int currentPatrolIndex = 0;
    public float lastAttackTime = 0f;
    public float lastJumpTime = 0f;

    private float sqrDetectionRange;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        patrolState = new PatrolState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        hitState = new HitState(this);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            player = playerObj.GetComponent<Hero2>();
        }

        sqrDetectionRange = detectionRange * detectionRange;
    }

    private void Start()
    {
        ChangeState(patrolState);
        animator.SetInteger("state", 1);
    }

    private void Update()
    {
        healthBar.value = Mathf.Lerp(healthBar.value, targetHealth, barAnimationSpeed * Time.deltaTime);
        if (currentHealth == 0) return;
        Debug.Log($"Changing the healthbar value to {targetHealth}");
        currentState?.Update();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentStateString = newState.ToString();
        currentState?.Enter();
    }

    public bool CanSeePlayer()
    {
        if (playerTransform == null) Debug.Log("NOOO PLAYER IS NULLL");
        if (playerTransform == null) return false;

        float sqrDistToPlayer = (transform.position - playerTransform.position).sqrMagnitude;
        if (sqrDistToPlayer > sqrDetectionRange) return false;

        Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, detectionRange, obstacleLayer);

        return hit.collider == null;
    }

    public bool IsPlayerInAttackRange()
    {
        if (playerTransform == null) return false;

        return Vector2.Distance(transform.position, playerTransform.position) <= attackRange;
    }

    public bool IsGrounded()
    {
        Vector2 groundCheckPosition = (Vector2)transform.position + Vector2.down * groundCheckDistance;
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPosition, Vector2.down, groundCheckDistance, groundLayer);

        return hit.collider != null;
    }

    public bool IsStuckInWall()
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider == null) return false;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleLayer);
        filter.useTriggers = false;

        Collider2D[] overlapping = new Collider2D[10];
        int count = Physics2D.OverlapCollider(myCollider, filter, overlapping);

        return count > 0;
    }

    public bool ShouldJump(Vector2 targetDirection)
    {
        if (!IsGrounded() || Time.time < lastJumpTime + jumpCooldown) return false;

        Vector2 obstacleCheckPosition = (Vector2)transform.position + targetDirection.normalized * obstacleCheckDistance;
        RaycastHit2D obstacleHit = Physics2D.Raycast(transform.position, targetDirection.normalized, obstacleCheckDistance, obstacleLayer);

        if (obstacleHit.collider == null) return false;

        Vector2 jumpCheckPosition = obstacleCheckPosition + Vector2.up * jumpCheckHeight;
        RaycastHit2D jumpSpaceHit = Physics2D.Raycast(obstacleCheckPosition, Vector2.up, jumpCheckHeight, obstacleLayer);

        return jumpSpaceHit.collider == null;
    }

    public bool ShouldJumpToReachPlayer()
    {
        if (playerTransform == null || !IsGrounded() || Time.time < lastAttackTime + jumpCooldown) return false; 

        if (playerTransform.position.y > transform.position.y + 1f)
        {
            float horizontalDistance = Mathf.Abs(playerTransform.position.x - transform.position.x);
            return horizontalDistance < detectionRange;
        }

        return false;
    }
    public void Jump(Vector2 direction = default)
    {
        if (!IsGrounded() || Time.time < lastJumpTime + jumpCooldown) return;

        if (direction == default)
            direction = Vector2.up;

        Vector2 jumpVelocity = new Vector2(direction.x * jumpForce * 0.5f, jumpForce);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + jumpVelocity.x, jumpVelocity.y);

        lastJumpTime = Time.time;

        animator.SetInteger("state", 2);
    }

    public void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        RaycastHit2D obstacleHit = Physics2D.Raycast(transform.position, direction, 1f, obstacleLayer);

        if (obstacleHit.collider != null)
        {
            Vector2 upDirection = direction + Vector2.up * 0.5f;
            RaycastHit2D upHit = Physics2D.Raycast(transform.position, upDirection.normalized, 1f, obstacleLayer);

            //if (upHit.collider == null && ShouldJump(direction))
            //{
            //    Jump(direction);
            //    return;
            //}
        }

        Vector2 targetVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 10f);

        if (direction.x > 0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);
            healthBar.direction = Slider.Direction.LeftToRight;
        }
        else if (direction.x < -0.1f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            healthBar.direction = Slider.Direction.RightToLeft;
        }
    }

    public void TakeDamage(float damage)
    {
        ChangeState(hitState);
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        targetHealth = currentHealth / maxHealth;
        Debug.Log($"target health is {targetHealth}");

        if (currentHealth == 0)
        {
            Destroy(gameObject, 1f);
        }
        else
        {
            ChangeState(hitState);
        }
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
