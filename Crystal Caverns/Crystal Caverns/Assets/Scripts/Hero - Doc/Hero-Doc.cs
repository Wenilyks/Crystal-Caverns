//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Hero : MonoBehaviour
//{
//    [SerializeField] private float speed = 3f;
//    [SerializeField] private int lives = 5;
//    [SerializeField] private float jumpForce = 10f;
//    [SerializeField] private float groundCheckRadius = 0.3f;
//    [SerializeField] private float wallSlideSpeed = 2f;
//    [SerializeField] private float wallJumpForce = 8f;
//    public BoxCollider2D groundCollider;
//    public BoxCollider2D wallLeftCollider;
//    public BoxCollider2D wallRightCollider;

//    private bool isGrounded = false;
//    private bool wasGrounded = false;
//    private bool isTouchingWall = false;
//    private bool isWallSliding = false;
//    private int wallDirection = 0; // -1 for left wall, 1 for right wall
//    private Rigidbody2D rb;
//    private Animator anim;
//    private SpriteRenderer sprite;

//    // Remove the State property to avoid conflicts

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        anim = GetComponent<Animator>();
//        sprite = GetComponentInChildren<SpriteRenderer>();
//    }

//    private void FixedUpdate()
//    {
//        CheckGround();
//    }

//    private void Update()
//    {
//        float moveInput = Input.GetAxis("Horizontal");

//        // Handle movement
//        if (moveInput != 0)
//        {
//            Run();
//        }

//        // Handle jumping - ground jump or wall jump
//        if (Input.GetButtonDown("Jump"))
//        {
//            if (isGrounded)
//            {
//                Jump();
//            }
//            else if (isWallSliding)
//            {
//                WallJump();
//            }
//        }

//        // Handle wall sliding
//        HandleWallSliding(moveInput);

//        // Update animations based on current state
//        UpdateAnimations(moveInput);
//    }

//    private void Run()
//    {
//        float moveInput = Input.GetAxis("Horizontal");

//        // Don't move horizontally if wall sliding and pushing into the wall
//        bool shouldMove = true;
//        if (isWallSliding)
//        {
//            bool pushingIntoWall = (wallDirection == 1 && moveInput > 0) || (wallDirection == -1 && moveInput < 0);
//            if (pushingIntoWall)
//            {
//                shouldMove = false; // Don't move into the wall
//            }
//        }

//        if (shouldMove)
//        {
//            // Use Rigidbody2D for physics-based movement instead of transform
//            rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
//        }

//        // Flip sprite based on movement direction
//        if (moveInput != 0)
//        {
//            Vector3 scale = transform.localScale;
//            scale.x = Mathf.Abs(scale.x) * (moveInput > 0 ? 1 : -1);
//            transform.localScale = scale;
//        }
//    }

//    private void Jump()
//    {
//        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
//        isGrounded = false; // Immediately set to false to prevent multiple jumps
//    }

//    private void WallJump()
//    {
//        // Jump away from the wall
//        Vector2 wallJumpDirection = new Vector2(-wallDirection, 1).normalized;
//        rb.velocity = new Vector2(0, 0); // Reset velocity first
//        rb.AddForce(wallJumpDirection * wallJumpForce, ForceMode2D.Impulse);

//        // Stop wall sliding
//        isWallSliding = false;
//    }

//    private void HandleWallSliding(float moveInput)
//    {
//        // Check if we should be wall sliding
//        if (!isGrounded && isTouchingWall && rb.velocity.y < 0)
//        {
//            // Check if player is pushing towards the wall
//            bool pushingTowardsWall = (wallDirection == 1 && moveInput > 0) || (wallDirection == -1 && moveInput < 0);

//            if (pushingTowardsWall)
//            {
//                isWallSliding = true;
//                // Limit the falling speed and stop horizontal movement into wall
//                rb.velocity = new Vector2(0, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
//            }
//            else
//            {
//                isWallSliding = false;
//            }
//        }
//        else
//        {
//            isWallSliding = false;
//        }
//    }

//    private void UpdateAnimations(float moveInput)
//    {
//        if (isWallSliding)
//        {
//            // Wall slide animation
//            anim.SetInteger("state", (int)States.wallSlide);
//        }
//        else if (!isGrounded)
//        {
//            // In air - jump animation
//            anim.SetInteger("state", (int)States.jump);
//        }
//        else if (Mathf.Abs(moveInput) > 0.1f)
//        {
//            // Moving on ground - walk animation
//            anim.SetInteger("state", (int)States.walk);
//        }
//        else
//        {
//            // Not moving on ground - idle animation
//            anim.SetInteger("state", (int)States.idle);
//        }
//    }

//    private void CheckGround()
//    {
//        wasGrounded = isGrounded;
//        isGrounded = Physics2D.OverlapCircle(groundCollider.transform.position, groundCheckRadius, LayerMask.GetMask("ground")) != null;

//        // Check wall collision and determine direction
//        bool isTouchingLeftWall = Physics2D.OverlapCircle(wallLeftCollider.transform.position, groundCheckRadius, LayerMask.GetMask("wall")) != null;
//        bool isTouchingRightWall = Physics2D.OverlapCircle(wallRightCollider.transform.position, groundCheckRadius, LayerMask.GetMask("wall")) != null;

//        isTouchingWall = isTouchingLeftWall || isTouchingRightWall;

//        // Determine wall direction
//        if (isTouchingLeftWall && !isTouchingRightWall)
//        {
//            wallDirection = -1; // Left wall
//        }
//        else if (isTouchingRightWall && !isTouchingLeftWall)
//        {
//            wallDirection = 1; // Right wall
//        }
//        else
//        {
//            wallDirection = 0; // No wall or both walls (shouldn't happen)
//        }
//    }

//    // Optional: Visual debugging in Scene view
//    private void OnDrawGizmos()
//    {
//        if (groundCollider != null)
//        {
//            Gizmos.color = isGrounded ? Color.green : Color.red;
//            Gizmos.DrawWireSphere(groundCollider.transform.position, groundCheckRadius);
//        }

//        if (wallLeftCollider != null)
//        {
//            Gizmos.color = isTouchingWall && wallDirection == -1 ? Color.yellow : Color.blue;
//            Gizmos.DrawWireSphere(wallLeftCollider.transform.position, groundCheckRadius);
//        }

//        if (wallRightCollider != null)
//        {
//            Gizmos.color = isTouchingWall && wallDirection == 1 ? Color.yellow : Color.blue;
//            Gizmos.DrawWireSphere(wallRightCollider.transform.position, groundCheckRadius);
//        }
//    }

//    public enum States
//    {
//        idle = 0,
//        walk = 1,
//        jump = 2,
//        wallSlide = 3
//    }
//}