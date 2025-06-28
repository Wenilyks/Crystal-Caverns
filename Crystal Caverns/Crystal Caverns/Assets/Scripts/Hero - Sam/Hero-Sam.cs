using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private float speed = 3f;
    [SerializeField] private int lives = 5;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform spriteHolder2;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Combat")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private float fireballSpeed = 8f;
    [SerializeField] private float attackCooldown = 0.7f;
    [SerializeField] private int rainFireballCount = 5;
    [SerializeField] private float rainSpread = 5f;
    [SerializeField] private float rainHeight = 3f;
    [SerializeField] private float groundPoundRadius = 3f;
    [SerializeField] private int groundPoundDamage = 25;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effects")]
    [SerializeField] private GameObject goundPoundEffect;
    [SerializeField] private ParticleSystem magicAura;

    private bool isGrounded = false;
    private bool canAttack = true;
    private bool isAttacking = false;

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

    private void FixedUpdate()
    {
        CheckGround();
    }

    private void Update()
    {
        if (isGrounded && !isAttacking)
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


        if (canAttack)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                State = States.idle;
                isAttacking = true;
                canAttack = false;
                State = States.attackTwo;
                StartCoroutine(PerformAttack(States.attackTwo));
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                // Start fireball rain attack 
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                // Start ground pound attack 
            }
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
            float animationLength = GetAnimationLength(attackType) / 1.3f;
            yield return new WaitForSeconds(animationLength);
            SpawnFireball();
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

            Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
            if (fireballRb != null)
            {
                fireballRb.linearVelocity = new Vector2(direction * fireballSpeed, 0);
            }

            Destroy(fireball, 5f);
        }
    }

    private float GetAnimationLength(States attackType)
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;

        string animationName = "";
        switch (attackType)
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