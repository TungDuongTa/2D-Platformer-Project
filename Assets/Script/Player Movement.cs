using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //overall
    private Rigidbody2D rb;
    //jump
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private Transform groundCheck;
    private Vector2 groundCheckSize = new Vector2(0.4f, 0.3f);
    [SerializeField] LayerMask groundLayer;
    private float maxFastFallSpeed = 50f;
    public float LastPressedJumpTime;
    public float LastOnGroundTime;
    public float yInput;
    public float xInput;
    public bool IsJumping;
    public bool IsFalling;
    public float gravityMultiplier;
    public float FallgravityMultiplier;
    public bool jumpCut;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        
    }

    // Update is called once per frame
    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        if (!IsJumping)
        {
            //Ground Check
            if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer) && !IsJumping) //checks if set box overlaps with ground
            {
                LastOnGroundTime = 0.15f; //if so sets the lastGrounded to coyoteTime
            }
        }


        LastOnGroundTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;


        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J))
        {

            LastPressedJumpTime = 0.1f;
        }
        //jump
        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            IsFalling = false;
            jumpCut = false;
            Jump();
        }
        if (IsJumping && rb.velocity.y < 0)
        {
            IsJumping = false;

        }
        if (LastOnGroundTime > 0 && !IsJumping)
        {
            jumpCut = false;
            if (!IsJumping)
            {
                IsFalling = false;
            }
        }
        //when player reach the max high of the jump
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityMultiplier;

            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFastFallSpeed));
        }
        else if (rb.velocity.y < 0 && yInput < 0)//holding down key case
        {
            //Much higher gravity if holding down
            rb.gravityScale = FallgravityMultiplier;
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFastFallSpeed));
        }
        else if (jumpCut)
        {
            rb.gravityScale = gravityMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFastFallSpeed));
        }
        else if ((IsJumping || IsFalling) && Mathf.Abs(rb.velocity.y) < 0.5f)
        {
            rb.gravityScale = gravityMultiplier;
        }

    }
        void Jump()
        {
            LastPressedJumpTime = 0;
            LastOnGroundTime = 0;
            float force = jumpForce;
            if (rb.velocity.y < 0)
            {
                force -= -rb.velocity.y;
            }
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }
        bool CanJump()
        {
            return LastOnGroundTime > 0 && !IsJumping;
        }
    
}
