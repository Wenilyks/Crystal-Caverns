using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FlyingEnemyController : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private float maxTransform;

    [Header("Animation settings")]
    [SerializeField] private float barAnimationSpeed = 0.7f;

    [Header("Detection")]
    public float detectionRange = 6f;
    public float attackRange = 2.5f;
    public LayerMask playerLayer = 1;
    public LayerMask obstacleLayer = 1;

    [Header("Movement")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;
    public float hoverHeight = 3f;
    public float verticalSpeed = 2f;
    public Transform[] patrolPoints;

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Flight Behaviour")]
    public float bobAmplitude = 0.5f;
    public float bobFrequency = 1f;
    public float obstacleAvoidanceDistance = 2f;
    public float maxFlightHeight = 8f;
    public float minFlightHeight = 1f;

    [Header("Components")]
    public Rigidbody2D rb { get; private set; }
    public Transform playerTransform { get; private set; }
    public Hero2 player { get; private set; }
    public Animator animator { get; private set; }

    [Header("State Machine")]
    public FlyingEnemyState currentState { get; private set; }
    public FlyingPatrolState patrolState { get; private set; } 
    public FlyingChaseState chaseState { get; private set; }
    public FlyingAttackState attackState { get; private set; }  
    public FlyingHitState hitState { get; private set; }
    public FlyingDeathState deathState { get; private set; }

    public string currentStateString = "patroleState";
    public int currentPatrolIndex = 0;
    public float lastAttackTime = 0f;

    private float sqrDetectionRange;
    private float startTime;
    private Vector2 basePosition;
    private float targetHealth = 100f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        rb.gravityScale = 0f;

        // init states
        patrolState = new FlyingPatrolState(this);
        chaseState = new FlyingChaseState(this);
        attackState = new FlyingAttackState(this);
        hitState = new FlyingHitState(this);
        deathState = new FlyingDeathState(this);
        

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            player = playerObj.GetComponent<Hero2>();
        }
        sqrDetectionRange = detectionRange * detectionRange;
        startTime = Time.time;
        basePosition = transform.position;
    }

    private void Start()
    {
        ChangeState(patrolState);
        animator.SetInteger("state", 1);
        maxTransform = healthBar.transform.localScale.x;
    }

    private void Update()
    {
        if (currentHealth <= 0 && currentState != deathState) return;
        healthBar.value = Mathf.Lerp(healthBar.value, targetHealth, barAnimationSpeed);
        currentState?.Update();
    }

    public void ChangeState(FlyingEnemyState state)
    {
        currentState?.Exit();
        currentState = state;
        currentStateString = state.ToString();
        currentState?.Enter();
    }

    public bool CanSeePlayer()
    {
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

    public Vector2 GetAvoidanceDirection(Vector2 targetDirection)
    {
        Vector2 avoidanceDirection = targetDirection;

        Vector2[] checkDirection =
        {
            targetDirection,
            targetDirection + Vector2.up * 0.5f,
            targetDirection + Vector2.down * 0.5f,
            targetDirection + Vector2.left * 0.5f,
            targetDirection + Vector2.right * 0.5f
        };

        foreach (var direction in checkDirection)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized,
                obstacleAvoidanceDistance, obstacleLayer);

            if (hit.collider == null)
            {
                avoidanceDirection = direction.normalized;
                break;
            }
        }

        return avoidanceDirection;
    }

    public void FlyTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        direction = GetAvoidanceDirection(direction);

        float targetY = Mathf.Clamp(target.y, minFlightHeight, maxFlightHeight);
        Vector2 adjustedTarget = new Vector2(target.x, targetY);
        direction = (adjustedTarget - (Vector2)transform.position).normalized;

        Vector2 targetVelocity = direction * speed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 8f);

        if (direction.x >= 0.1f)
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

    public void Hover()
    {
        float bobOffset = Mathf.Sin((Time.time - startTime) * bobFrequency) * bobAmplitude;
        Vector2 hoverTarget = basePosition + Vector2.up * bobOffset;

        Vector2 direction = (hoverTarget - (Vector2)transform.position).normalized;
        Vector2 targetVelocity = direction * (patrolSpeed * 0.5f);

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 5f);
    }

    public void TakeDamage(float damage)
    {
        ChangeState(hitState);
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        targetHealth = currentHealth / 100f;

        if (currentHealth == 0)
        {
            ChangeState(deathState);
            Destroy(gameObject, 1f);
        }
        else
        {
            ChangeState(hitState);
        }
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 10f);
    }

    public void SetBasePosition(Vector2 position)
    {
        basePosition = position;
    }
    private void OnDrawGizmos()
    {
        // Detection range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Flight height constraints
        Gizmos.color = Color.cyan;
        Vector3 pos = transform.position;
        Gizmos.DrawLine(new Vector3(pos.x - 1f, minFlightHeight, pos.z),
                       new Vector3(pos.x + 1f, minFlightHeight, pos.z));
        Gizmos.DrawLine(new Vector3(pos.x - 1f, maxFlightHeight, pos.z),
                       new Vector3(pos.x + 1f, maxFlightHeight, pos.z));

        //currentState?.OnDrawGizmos();
    }
}
