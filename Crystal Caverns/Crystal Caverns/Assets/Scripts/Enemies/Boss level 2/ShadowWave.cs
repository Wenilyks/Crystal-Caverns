using UnityEngine;

public class ShadowWave : MonoBehaviour
{
    private float speed;
    private Vector2 direction;
    private float damage;
    private Rigidbody2D rb;
    private float lifetime = 3f;
    private bool hasHitPlayer = false;

    public void Initialize(float waveSpeed, Vector2 waveDirection, float waveDamage)
    {
        speed = waveSpeed;
        direction = waveDirection.normalized;
        damage = waveDamage;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        StartCoroutine(ScaleWave());

        Destroy(gameObject, lifetime);
    }

    private System.Collections.IEnumerator ScaleWave()
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 2f;
        float scaleTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < scaleTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / scaleTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasHitPlayer)
        {
            hasHitPlayer = true;
            Hero2 player = other.GetComponent<Hero2>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            AudioManager.Instance?.PlaySFX("Shadow_Wave_Hit");
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject, 2f);
        }
    }
}