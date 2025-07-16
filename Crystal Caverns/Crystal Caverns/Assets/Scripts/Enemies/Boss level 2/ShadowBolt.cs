using UnityEngine;

public class ShadowBolt : MonoBehaviour
{
    private float speed;
    private Vector2 direction;
    private float damage;
    private Rigidbody2D rb;
    private float lifetime = 5f;
    private float hitTimeCooldown = 2f;
    private float hitTimerCooldown = 2f;
    private bool isUsedByPlayer = false;

    public void Initialize(float boltSpeed, Vector2 boltDirection, float boltDamage, bool isUsedByPlayer = false)
    {
        speed = boltSpeed;
        direction = boltDirection.normalized;
        damage = boltDamage;
        this.isUsedByPlayer = isUsedByPlayer;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocityY);


        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isUsedByPlayer)
        {
            if (hitTimerCooldown >= hitTimeCooldown)
            {
                Hero2 player = other.GetComponent<Hero2>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
                hitTimerCooldown = 0f;
                AudioManager.Instance?.PlaySFX("Shadow_Impact");

                GetComponentInChildren<Animator>().SetTrigger("endAttack");
                Destroy(gameObject, 3f);
            }

            hitTimerCooldown += Time.deltaTime; 

        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject, 3f);
        }
        else if (other.CompareTag("Enemy"))
        {
            if (hitTimerCooldown >= hitTimeCooldown)
            {
                IDamageable enemy = other.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                hitTimerCooldown = 0f;
                AudioManager.Instance?.PlaySFX("Shadow_Impact");

                GetComponentInChildren<Animator>().SetTrigger("endAttack");
                Destroy(gameObject, 3f);
            }

            hitTimerCooldown += Time.deltaTime;
        }
    }
}