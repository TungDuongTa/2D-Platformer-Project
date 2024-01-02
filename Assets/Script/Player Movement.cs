using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //input
    [SerializeField] private float yRawInput;
    [SerializeField] private float xRawInput;
    [SerializeField] private float yInput;
    [SerializeField] private float xInput;
    //overall
    private Rigidbody2D rb;
    private Vector2 groundCheckSize = new Vector2(1.3f, 0.2f);
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 1.5f);
    //jump
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform frontWallCheck;
    [SerializeField] private Transform backWallCheck;

    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] private float maxFastFallSpeed;
    [SerializeField] private float LastPressedJumpTime;
    [SerializeField] private float LastOnGroundTime;
    

    [SerializeField] private float gravityMultiplier;
    [SerializeField] private float FallgravityMultiplier;

    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float FastFallgravityMultiplier;
    [SerializeField] private float JumpCutMultiplier;

    [SerializeField] private float runMaxSpeed;
    [SerializeField] private float RunAccel;
    [SerializeField] private float RunDeccel;

    [SerializeField] private SpriteRenderer sr;
    public float targetSpeed;
    public float accelRate;
    public float speedDif;
    public float movement;
    public bool IsJumping;
    public bool IsFalling;
    public bool JumpCut;
    public float CoyoteTime;
    public float JumpBufferTime;
    public float force;
    public bool IsSliding;
    public float LastOnWallRightTime;
    public float LastOnWallLeftTime;
    public float LastOnWallTime;
    public bool IsWallJumping;
    public bool IsDashing;
    
    private enum movementState { idle, running, jumping, falling,wallJump }
    movementState state;
    private Animator playerAni;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr= GetComponent<SpriteRenderer>();
        playerAni = GetComponent<Animator>();

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    
    void Update()
    {
        if (CanSlide() && ((LastOnWallLeftTime > 0 && xRawInput < 0) || (LastOnWallRightTime > 0 && xRawInput > 0)))
            IsSliding = true;
        else
            IsSliding = false;
        KeyInput();
        animationMovement();
        if (!IsJumping)
        {
            //Ground Check
            if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer) && !IsJumping) //checks if set box overlaps with ground
            {
                LastOnGroundTime = CoyoteTime; //if so sets the lastGrounded to coyoteTime
            }
            //Right Wall Check
            if (((Physics2D.OverlapBox(frontWallCheck.position, wallCheckSize, 0, wallLayer) && !sr.flipX)
                    || (Physics2D.OverlapBox(backWallCheck.position, wallCheckSize, 0, wallLayer) && sr.flipX)) )
                LastOnWallRightTime = CoyoteTime;

            //Right Wall Check
            if (((Physics2D.OverlapBox(frontWallCheck.position, wallCheckSize, 0, wallLayer) && !sr.flipX)
                || (Physics2D.OverlapBox(backWallCheck.position, wallCheckSize, 0, wallLayer) && sr.flipX)) )
                LastOnWallLeftTime = CoyoteTime;
            //Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }


        LastOnGroundTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;


        if (Input.GetButtonDown("Jump"))
        {
            
            LastPressedJumpTime = JumpBufferTime;
        }
        if (Input.GetButtonUp("Jump"))
        {
            if (CanJumpCut()) {
                JumpCut = true;
            }
            
        }
        //jump
        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            IsFalling = false;
            JumpCut = false;
            
            Jump();
        }
        if (IsJumping && rb.velocity.y < 0)
        {
            IsJumping = false;
            IsFalling = true;

        }
        if (LastOnGroundTime > 0 && !IsJumping)
        {
            JumpCut = false;
            
            IsFalling = false;
            
        }

        if (IsSliding) {
            rb.gravityScale = 0;
        }
        // note that in if else block statement , if 2 cases that have 1 same condition, place the case that have more condition above. if you do the opposite, it will not work
        else if (rb.velocity.y < 0 && yRawInput < 0)//holding down key case, gravity scale= 2.5*2=5 max fast fall speed =30
        {

            rb.gravityScale = 2.5f * FastFallgravityMultiplier;

            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFastFallSpeed));
        }
        else if (JumpCut)//when the jump button is release,gravity scale= 2.5*2=5,max fast fall speed =30
        {
            rb.gravityScale = 2.5f * JumpCutMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFastFallSpeed));
        }
        else if ((IsJumping) && Mathf.Abs(rb.velocity.y) < 5)//increase air time, also called jump hang
        {
            rb.gravityScale = 1.25f;
        }
        else if (rb.velocity.y < 0)//when player reach the max high of the jump,gravity scale = 2.5 * 1.5 =3,75, max fall speeed=25f
        {

            rb.gravityScale = 2.5f * FallgravityMultiplier;

            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = 2.5f;
        }


        
    }
    #region JUMP
    void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        force = jumpForce;
        if (rb.velocity.y < 0)
        {
            force -= rb.velocity.y;
        }
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }
    #endregion
    bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }
    void KeyInput()
    {
        xRawInput = Input.GetAxisRaw("Horizontal");
        yRawInput = Input.GetAxisRaw("Vertical");
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
    }
    private void Run(float lerpAmount)
    {
        //Calculate the direction we want to move in and our desired velocity
         targetSpeed = xRawInput * runMaxSpeed;

        //We can reduce are control using Lerp() this smooths changes to are direction and speed
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount);
        //
        
        if (LastOnGroundTime > 0)
            if (Mathf.Abs(targetSpeed) > 0.01f)
            {
                accelRate = RunAccel;
            }
            else
            {
                accelRate = RunDeccel;
            }

        else {
            if (Mathf.Abs(targetSpeed) > 0.01f)
            {
                accelRate = RunAccel * 0.65f;
            }
            else
            {
                accelRate = RunDeccel * 0.65f;
            }
        }
        #region Add Bonus Jump Apex Acceleration
        //Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
        if ((IsJumping || IsFalling) && Mathf.Abs(rb.velocity.y) < 5)
        {
            accelRate *= 1.1f;
            targetSpeed *= 1.3f;
        }
        #endregion

        #region Conserve Momentum
        //We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
        if (Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            //Prevent any deceleration from happening, or in other words conserve are current momentum
            //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
            accelRate = 0;
        }
        #endregion

        //Calculate difference between current velocity and desired velocity
         speedDif = targetSpeed - rb.velocity.x;
        //Calculate force along x-axis to apply to thr player

         movement = speedDif * accelRate;

        //Convert this to a vector and apply to rigidbody
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        /*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
    }
    private bool CanJumpCut()
    {
        return IsJumping && rb.velocity.y > 0;
    }
    private void FixedUpdate()
    {
        //Handle Run

        Run(1);
        if (IsSliding)
            Slide();


    }

    void animationMovement() {
        if (xRawInput > .1f)
        {
            sr.flipX = false;
            state = movementState.running;
        }
        else if (xRawInput < 0)
        {
            sr.flipX = true;
            state = movementState.running;
        }
        else
        {
            state = movementState.idle;
        }
        if (rb.velocity.y > .1f)
        {
            state = movementState.jumping;
        }

        else if (rb.velocity.y < 0 && rb.gravityScale == 0f)
        {
            state = movementState.wallJump;
        }
        else if (rb.velocity.y < 0)
        {
            state = movementState.falling;
        }
        else if (rb.velocity.y == 0 && rb.velocity.x == 0) {
            state = movementState.idle;
        }
        playerAni.SetInteger("state", (int)state);
    }
    public bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && LastOnGroundTime <= 0)
            return true;
        else
            return false;
    }

    private void Slide()
    {
        //We remove the remaining upwards Impulse to prevent upwards sliding
        if (rb.velocity.y > 0)
        {
            rb.AddForce(-rb.velocity.y * Vector2.up, ForceMode2D.Impulse);
        }

        //Works the same as the Run but only in the y-axis
        //THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
        float speedDif = 0 - rb.velocity.y;
        float movement = speedDif * 0;
        //So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
        //The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        rb.AddForce(movement * Vector2.up);
    }

}

