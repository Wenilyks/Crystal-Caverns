using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero2 : Entity
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private int lives = 5;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform spriteHolder2; // 👉 ссылка на SpriteHolder
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private bool isGrounded = false;

    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCollider;
    public static Hero2 Instance { get; set; }
    // Значение смещения коллайдера при повороте
    private float colliderOffsetX;

    private States State
    {
        get { return (States)anim.GetInteger("state"); }
        set { anim.SetInteger("state", (int)value); }
    }

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = spriteHolder2.GetComponent<Animator>(); // Animator на дочернем объекте
        colliderOffsetX = boxCollider.offset.x; // Запоминаем оригинальное смещение
    }

    private void FixedUpdate()
    {
        CheckGround();
    }

    private void Update()
    {
        if (isGrounded) State = States.idle;

        if (Input.GetButton("Horizontal"))
            Run();

        if (isGrounded && Input.GetButtonDown("Jump"))
            Jump();
    }

    private void Run()
    {
        if (isGrounded) State = States.run;

        float moveInput = Input.GetAxis("Horizontal");
        Vector3 dir = transform.right * moveInput;

        transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, speed * Time.deltaTime);

        if (moveInput != 0)
        {
            // Отражаем только спрайт
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



    private void CheckGround()
{
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    

    if (!isGrounded)
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


    public enum States
    {
        idle,
        run,
        jump
    }
    public override void GetDamage()
    {
        lives -= 10;
        Debug.Log(lives);
    }

}
