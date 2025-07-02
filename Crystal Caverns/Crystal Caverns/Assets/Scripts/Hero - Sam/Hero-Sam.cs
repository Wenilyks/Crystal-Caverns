using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum States
{
    idle,
    run,
    jump,
    death,
    hit,
    attackOne,
    attackTwo,
    attackThree
}

public class Hero2 : MonoBehaviour
{
    [Header("Player stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float maxMagicAura = 100f;
    [SerializeField] private float currentMagicAura = 100f;
    [SerializeField] private float magicRegenRate = 5f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private int lives = 5;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("UI")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image magicAuraBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text magicAuraText;

    [Header("Animation settings")]
    [SerializeField] private float barAnimationSpeed = 0.1f;
    [SerializeField] private float barAnimationDuration = 0.5f;

    private float targetHealthValue = 1f;
    private float targetMagicValue = 1f;


    [Header("Combat")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject fireballPrefab2;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private float fireballSpeed = 8f;
    [SerializeField] private float attackCooldown = 0.7f;
    [SerializeField] private int rainFireballCount = 5;
    [SerializeField] private float rainSpread = 5f;
    [SerializeField] private float rainHeight = 3f;
    [SerializeField] private float groundPoundRadius = 3f;
    [SerializeField] private int groundPoundDamage = 25;
    [SerializeField] private float fireballMagicCost = 20f;
    [SerializeField] private float rainFireballMagicCost = 25f;
    [SerializeField] private float groundPoundMagicCost = 25f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effects")]
    [SerializeField] private GameObject groundPoundEffect;
    [SerializeField] private ParticleSystem magicAura;

    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform spriteHolder2;
    [SerializeField] private Transform groundCheck;

    private bool isGrounded = false;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool isGettingHit = false;

    private Rigidbody2D rb;
    private Animator anim;
    public static Hero2 Instance { get; set; }

    private States State
    {
        get { return (States)anim.GetInteger("state"); }
        set { anim.SetInteger("state", (int)value); }
    }

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        anim = spriteHolder2.GetComponent<Animator>();
    }

    private void Start()
    {
        if (healthBar != null) healthBar.fillAmount = 1f;
        if (magicAuraBar != null) magicAuraBar.fillAmount = 1f;
        UpdateTexts();
    }

    private void FixedUpdate()
    {
        CheckGround();
    }

    private void Update()
    {
        Debug.Log(currentMagicAura);
        if (isGrounded && !isAttacking && !isGettingHit)
        {
            Debug.Log("Idle state lol");
            State = States.idle;
        }
        if (!isAttacking)
        {
            if (Input.GetButton("Horizontal"))
                Run();

            if (isGrounded && Input.GetButtonDown("Jump"))
                Jump();
        }

        if (currentMagicAura < maxMagicAura)
        {
            currentMagicAura += magicRegenRate * Time.deltaTime;
            currentMagicAura = Mathf.Clamp(currentMagicAura, 0, maxMagicAura);
            targetMagicValue = currentMagicAura / maxMagicAura;
        }

        if (healthBar != null)
        {
            healthBar.DOFillAmount(targetHealthValue, barAnimationDuration);
        }

        if (magicAuraBar != null)
        {
            magicAuraBar.DOFillAmount(targetMagicValue, barAnimationDuration);  
        }

        if (canAttack)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                State = States.idle;
                if (!(currentMagicAura >= fireballMagicCost)) return;
                isAttacking = true;
                canAttack = false;
                State = States.attackTwo;
                StartCoroutine(PerformAttack(States.attackTwo));
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                if (!(currentMagicAura >= rainFireballMagicCost)) return;
                isAttacking = true;
                canAttack = false;
                State = States.attackOne;
                StartCoroutine (PerformAttack(States.attackOne));
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                if (!(currentMagicAura >= groundPoundMagicCost)) return;
                isAttacking = true;
                canAttack = false;
                State = States.attackThree;
                StartCoroutine(PerformAttack(States.attackThree));
            }
        }
        UpdateTexts();
    }

    public void RestoreMagic(float magicAmount)
    {
        currentMagicAura += magicAmount;
        currentMagicAura = Mathf.Clamp(currentMagicAura, 0, maxMagicAura);
        targetMagicValue = currentMagicAura / maxMagicAura;
    }

    private void UpdateTexts()
    {
        if (healthText != null)
        {
            healthText.text = $"{(int)currentHealth} / {(int)maxHealth}";
        }

        if (magicAuraText != null)
        {
            magicAuraText.text = $"{(int)currentMagicAura} / {(int)maxMagicAura}";
        }
    }
    private void Run()
    {
        if (isGrounded) State = States.run;

        float moveInput = Input.GetAxis("Horizontal");
        Vector3 dir = transform.right * moveInput;

        transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, speed * Time.deltaTime);

        if (moveInput != 0)
        {
            Vector3 scale = spriteHolder2.localScale;
            scale.x = Mathf.Abs(scale.x) * (moveInput > 0 ? 1 : -1);
            spriteHolder2.localScale = scale;
        }
    }

    private void Jump()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private IEnumerator PerformAttack(States attackType)
    {
        isAttacking = true;
        canAttack = false;
        if (attackType == States.attackTwo)
        {
            float animationLength = GetAnimationLength(attackType) / 1.7f;
            yield return new WaitForSeconds(animationLength);
            SpawnFireball();
            currentMagicAura -= fireballMagicCost;
        }

        else if (attackType == States.attackOne)
        {
            float animationLength = GetAnimationLength(attackType);
            yield return new WaitForSeconds(animationLength);
            StartCoroutine(SpawnFireballRain());
            currentMagicAura -= rainFireballMagicCost;
        }

        else if (attackType == States.attackThree)
        {
            float animationLength = GetAnimationLength(attackType);
            yield return new WaitForSeconds(animationLength);
            StartCoroutine(GroundPound());
            currentMagicAura -= groundPoundMagicCost;
        }

        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void SpawnFireball()
    {
        if (fireballPrefab != null && fireballSpawnPoint != null)
        {
            GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);

            float direction = spriteHolder2.localScale.x > 0 ? 1 : -1;

            fireball.transform.localScale = new Vector3(0.4f * direction, 0.4f, 0.4f);

            Fireball fireballScript = fireball.GetComponent<Fireball>();
            if (fireballScript != null)
            {
                fireballScript.Initialize(fireballSpeed, direction, true);
            }

            Destroy(fireball, 5f);
        }
    }

    public IEnumerator SpawnFireballRain()
    {
        Debug.Log("HERE");
        if (fireballPrefab != null)
        {
            Vector3 playerPosition = transform.position;

            for (int i = 0; i < rainFireballCount; i++)
            {
                float xOffset;
                if (i % 2 == 0)
                {
                    xOffset = UnityEngine.Random.Range(0f, rainSpread);
                }
                else
                {
                    xOffset = -UnityEngine.Random.Range(0f, rainSpread);
                }

                Vector3 spawnPoint = new Vector3(
                    playerPosition.x + xOffset,
                    playerPosition.y + rainHeight,
                    0
                );

                GameObject fireball = Instantiate(fireballPrefab2, spawnPoint, Quaternion.identity);

                fireball.transform.localRotation = Quaternion.Euler(0, 0, -90f);

                Debug.Log($"Fireball spawned at: {spawnPoint}");

                Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
                if (fireballRb != null)
                {
                    Vector2 fallVelocity = new Vector2(
                        Random.Range(-2f, 2f),
                        -fireballSpeed * 0.7f
                    );
                    fireballRb.linearVelocity = fallVelocity;
                }

                Destroy(fireball, 4f);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private IEnumerator GroundPound()
    {
        if (groundPoundEffect != null)
        {
            GameObject effect = Instantiate(groundPoundEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (magicAura != null)
        {
            magicAura.Play();
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, groundPoundRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            IDamageable enemyScript = enemy.GetComponent<IDamageable>();
            if (enemyRb != null)
            {
                enemyScript.TakeDamage(groundPoundDamage);
                Vector2 knowbackDirection = (enemy.transform.position - transform.position).normalized;
                enemyRb.AddForce(knowbackDirection * 40, ForceMode2D.Impulse);
            }
        }

        yield return new WaitForSeconds(2f);
    }

    public void TakeDamage(float damage)
    {
        State = States.hit;
        isGettingHit = true;

        Debug.Log("Player is taking damage");

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        targetHealthValue = currentHealth / maxHealth;
        StartCoroutine(TakeDamage());
        Update();

        if (currentHealth == 0)
        {
            Destroy(gameObject);
            return;
        }

    }

    private IEnumerator TakeDamage()
    {
        Debug.Log($"current state is {State.ToString()}");
        float animationLength = GetAnimationLength(States.hit);
        yield return new WaitForSeconds(animationLength);
        isGettingHit = false;
    }

    private float GetAnimationLength(States state)
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;

        string animationName = "";
        switch (state)
        {
            case States.attackOne:
                animationName = "attackOne"; 
                break;
            case States.attackTwo:
                animationName = "attackTwo";
                break;
            case States.attackThree:
                animationName = "attackThree";
                break;
            case States.hit:
                animationName = "hit";
                break;
        }

        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }

        return 1f; 
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isGrounded && !isAttacking)
            State = States.jump;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void GetDamage()
    {
        lives -= 10;
        Debug.Log(lives);
    }
}