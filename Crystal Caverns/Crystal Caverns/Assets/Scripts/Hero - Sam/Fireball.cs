using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Fireball : MonoBehaviour
{
    [SerializeField] private int damage = 15;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float turnSpeed = 3f;
    
    private bool hasExploded = false;
    private float currentSpeed;
    private float initialDirection;
    private Transform targetEnemy;
    private bool aim = false;

    private Transform specificTarget;
    private string specificTargetTag;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();   
    }

    public void Initialize(float speed, float direction, bool aim, Transform specificTarget = null)
    {
        currentSpeed = speed;
        initialDirection = direction;
        this.aim = aim;
        this.specificTarget = specificTarget;   

        if (rb != null)
        {
            if (direction == 0)
            {
                Vector2 fallVelocity = new Vector2(
                        UnityEngine.Random.Range(-2f, 2f),
                        -8f * 0.7f
                    );
                rb.linearVelocity = fallVelocity;
            }
            else 
                rb.linearVelocity = new Vector2(direction * speed, 0);
        }
    }

    private void Update()
    {
        if (hasExploded || rb == null || !aim) return;

        if (specificTarget == null)
            FindNearestEnemy();
        else
        {
            targetEnemy = specificTarget;
            specificTargetTag = specificTarget.gameObject.tag;
        }

        if (targetEnemy != null)
        {
            MoveTowardsTarget();
        }

        else
        {
            rb.linearVelocity = new Vector2(initialDirection * currentSpeed, rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded) return;

        int layer = collision.gameObject.layer;
        if (((1 << layer) & enemyLayer) != 0 || ((1 << layer) & groundLayer) != 0 || (((1 << layer) & targetLayer) != 0 && specificTarget != null))
        {
            Explode();
        }
    }

    private void FindNearestEnemy()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

        if (enemiesInRange.Length == 0)
        {
            targetEnemy = null;
            return;
        }

        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (var enemy in  enemiesInRange)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = enemy.transform;
            }
        }

        targetEnemy = closestTarget;
    }

    private void MoveTowardsTarget()
    {
        if (targetEnemy == null) return;

        Vector2 directionToTarget = (targetEnemy.position - transform.position).normalized;
        Vector2 currentVelocity = rb.linearVelocity.normalized;

        Vector2 newDirection = Vector2.Lerp(currentVelocity, directionToTarget, turnSpeed * Time.deltaTime);

        rb.linearVelocity = newDirection * currentSpeed;

        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(initialDirection > 0 ? angle : -angle, Vector3.forward);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;
        Explode();
    }

    private void Explode()
    {
        AudioManager.Instance.PlaySFX("Sphere explosion");
        hasExploded = true;

        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }


        if (specificTarget == null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
            foreach (Collider2D enemy in hitEnemies)
            {
                Debug.Log("enemy is taking damage");

                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                IDamageable enemyController = enemy.GetComponent<IDamageable>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(damage);
                }
                if (enemyRb != null)
                {
                    Debug.Log("Pushing back");
                    Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDirection * 2f, ForceMode2D.Impulse);
                }
            }
        }
        else
        {
            Debug.Log("I will explode in target");
            Collider2D[] hitTarget = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetLayer);
            foreach (Collider2D target in hitTarget)
            {
                Debug.Log($"Our target is: {target.gameObject.tag}");

                Rigidbody2D enemyRb = target.GetComponent<Rigidbody2D>();
                IDamageable enemyController = target.GetComponentInParent<IDamageable>();
                Debug.Log("Checking if target controller is not equals to null");
                if (enemyController != null)
                {
                    Debug.Log("target is taking damage");
                    enemyController.TakeDamage(damage);
                }
                else
                {
                    Debug.Log("target controller is null");
                }
                if (enemyRb != null)
                {
                    Debug.Log("Pushing back");
                    Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDirection * 2f, ForceMode2D.Impulse);
                }
            }
        }

        Destroy(gameObject);
    }
}