using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private int lives = 5;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform spriteHolder; // 👉 ссылка на SpriteHolder

    private bool isGrounded = false;

    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCollider;

    // Значение смещения коллайдера при повороте
    private float colliderOffsetX;

    private States State
    {
        get { return (States)anim.GetInteger("state"); }
        set { anim.SetInteger("state", (int)value); }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = spriteHolder.GetComponent<Animator>(); // Animator на дочернем объекте
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
            Vector3 scale = spriteHolder.localScale;
            scale.x = Mathf.Abs(scale.x) * (moveInput > 0 ? 1 : -1);
            spriteHolder.localScale = scale;

            // Смещаем коллайдер в зависимости от направления
            Vector2 offset = boxCollider.offset;
            offset.x = colliderOffsetX * (moveInput > 0 ? 1 : -1);
            boxCollider.offset = offset;
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
        isGrounded = Physics2D.OverlapCircle(transform.position, 0.3f, groundLayer);

        if (!isGrounded)
            State = States.jump;
    }


    public enum States
    {
        idle,
        run,
        jump
    }
}
