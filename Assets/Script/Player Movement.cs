using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //input
    private float yRawInput;
    private float xRawInput;
    private float yInput;
    private float xInput;
    //overall
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private enum movementState { idle, running, jumping, falling, wallJump }
    movementState state;
    private Animator playerAni;
    [Header("Check Size/Layer")]
    private Vector2 groundCheckSize = new Vector2(1.3f, 0.2f);
    private Vector2 wallCheckSize = new Vector2(0.1f, 1.5f);
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform frontWallCheck;
    [SerializeField] private Transform backWallCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask wallLayer;
    [Header("Jump/Gravity")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float force;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float LastPressedJumpTime;
    [SerializeField] private float LastOnGroundTime;
    [SerializeField] private float maxFastFallSpeed;
    [SerializeField] private float FallgravityMultiplier;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float FastFallgravityMultiplier;
    [SerializeField] private float JumpCutMultiplier;
    [SerializeField] private float CoyoteTime;
    [SerializeField] private float JumpBufferTime;
    [SerializeField] private float gravityStrength;
    [SerializeField] private float jumpTimeToApex;
    [SerializeField] private float gravityScale;
    [Header("Run")]
    [SerializeField] private float runMaxSpeed;
    [SerializeField] private float RunAccel;
    [SerializeField] private float RunDeccel;
    [SerializeField] private float targetSpeed;
    [SerializeField] private float accelRate;
    [SerializeField] private float speedDif;
    [SerializeField] private float movement;
    [Header("Bool")]
    public bool IsJumping;
    public bool IsFalling;
    public bool IsJumpCut;
    public bool IsSliding;
    public bool IsWallJumping;
    public bool IsDashing;
    [Header("Wall Slide")]
    [SerializeField] private float LastOnWallRightTime;
    [SerializeField] private float LastOnWallLeftTime;
    [SerializeField] private float LastOnWallTime;
    //Wall Jump
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    public bool IsFacingRight;
    public bool WallGrab;
    private void OnValidate()
    {
        //all these function below is to ensure that player can reach the desired height(jumpHeight) in desired time(jumpTimeToApex)
        //by modify gravity scale in rigidbody2d
        //Calculate gravity strength 
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
        //Calculate the rigidbody's gravity scale (
        gravityScale = gravityStrength / Physics2D.gravity.y;
        //Calculate jumpForce 
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerAni = GetComponent<Animator>();

    }
    // Start is called before the first frame update
    void Start()
    {
        IsFacingRight = true;
    }

    private void FixedUpdate()
    {
        //Handle Run

        if (IsWallJumping)
            Run(0.5f);
        else
            Run(1f);
        if (IsSliding)
            Slide();


    }
    void Update()
    {



        if (CanSlide() && ((LastOnWallTime > 0 && xRawInput != 0)))
            IsSliding = true;
        else
            IsSliding = false;
        GroundWallCheck();
        KeyInput();
        animationMovement();
        GravityScale();



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
            if (CanJumpCut())
            {
                IsJumpCut = true;
            }

        }
        if (Input.GetButton("Fire1") && LastOnWallTime > 0)
        {
            WallGrab = true;
            IsSliding = false;
        }
        else if (Input.GetButtonUp("Fire1") || LastOnWallTime < 0) { 
            WallGrab = false;
            IsSliding = false;
        }
        if (WallGrab)
        {
            rb.gravityScale = 0;

            // If horizontal input is significant, stop horizontal movement
            if (Mathf.Abs(xInput) > 0.1f)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }

            float WallSlideSpeed;
            float WallSlideaccelRate = 3f;
            // If vertical input is positive, move up the wall; if negative, move down the wall
            WallSlideSpeed = yInput > 0 ? 5f : -10f;

            // If no vertical input, set WallSlideSpeed to 0 to stay still
            if (Mathf.Abs(yInput) < 0.1f)
            {
                WallSlideSpeed = 0f;
            }

            WallSlideSpeed = Mathf.Lerp(rb.velocity.y, WallSlideSpeed, 1);
            float WallspeedDifference = WallSlideSpeed - rb.velocity.y;
            float WallslideMovement = WallspeedDifference * WallSlideaccelRate;
            rb.AddForce(Vector2.up * WallslideMovement, ForceMode2D.Force);
        }
        //jump
        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            IsWallJumping = false;
            IsFalling = false;
            IsJumpCut = false;

            Jump();
        }//WALL JUMP
        else if (CanWallJump() && LastPressedJumpTime > 0)
        {
            IsWallJumping = true;
            IsJumping = false;
            IsJumpCut = false;
            IsFalling = false;

            _wallJumpStartTime = Time.time;
            _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

            WallJump(_lastWallJumpDir);
        }


        if (IsJumping && rb.velocity.y < 0)
        {
            IsJumping = false;
            IsFalling = true;

        }
        if (IsWallJumping && Time.time - _wallJumpStartTime > 0.15)
        {
            IsWallJumping = false;
        }
        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
            IsJumpCut = false;

            IsFalling = false;

        }





    }
    #region gravity scale and jump scale
    private void GravityScale() {
        if (IsSliding)
        {
            rb.gravityScale = 0;
        }
        // note that in if else block statement , if 2 cases that have 1 same condition, place the case that have more condition above. if you do the opposite, it will not work
        else if (rb.velocity.y < 0 && yRawInput < 0)//holding down key case, gravity scale= 2.5*2=5 max fast fall speed =30
        {

            rb.gravityScale = gravityScale * FastFallgravityMultiplier;

            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFastFallSpeed));
        }
        else if (IsJumpCut)//when the jump button is release,gravity scale= 2.5*2=5,max fast fall speed =30
        {
            rb.gravityScale = gravityScale * JumpCutMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFastFallSpeed));
        }
        else if ((IsJumping || IsFalling || IsWallJumping) && Mathf.Abs(rb.velocity.y) < 5)//increase air time, also called jump hang
        {
            rb.gravityScale = gravityScale / 2;
        }
        else if (rb.velocity.y < 0)//when player reach the max high of the jump,gravity scale = 2.5 * 1.5 =3,75, max fall speeed=25f
        {

            rb.gravityScale = gravityScale * FallgravityMultiplier;

            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }
    #endregion\

    #region JUMP
    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }
    private bool CanJumpCut()
    {
        return IsJumping && rb.velocity.y > 0;
    }
    private void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        //jumpForce = Mathf.Sqrt(jumpHeight * (Physics2D.gravity.y * rb.gravityScale) * -2) * rb.mass;//ensure the jumpheight will be the same
        force = jumpForce;
        if (rb.velocity.y < 0)//during coyote time, when player falling , we have velocity.y downward->lower jump. With this code, we can make the jump feel the same
        {
            force -= rb.velocity.y;
        }
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }
    #endregion

    #region Run and also Jump Apex Gravity
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

        else
        {
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
        if ((IsJumping || IsFalling) && Mathf.Abs(rb.velocity.y) < 20)
        {
            accelRate *= 1.1f;
            targetSpeed *= 1.3f;
        }
        #endregion

        #region Conserve Momentum
        //We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
        /*if (Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            //Prevent any deceleration from happening, or in other words conserve are current momentum
            //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
            accelRate = 0;
        }*/
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
    #endregion

    #region Animaiton Handling
    void animationMovement()
    {
        if (xRawInput !=-0f|| xRawInput != -0 && rb.velocity.y == 0)
        {
            Flip();
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

        else if (rb.velocity.y != 0 && LastOnWallTime > 0 && xRawInput != 0)
        {
            state = movementState.wallJump;
        }
        else if (rb.velocity.y < 0)
        {
            state = movementState.falling;
        }

        playerAni.SetInteger("state", (int)state);
        

    }
    #endregion

    #region Wall Slide
    public bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && LastOnGroundTime <= 0)
            return true;
        else
            return false;
    }
    //this code will allow you to slide  at different speed base on how you land to the wall
    /*private void Slide()
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
    }*/
    //this code will alllow you to slide to a targeted speed 
    private void Slide()//same as horizontal move, it just verticle
    {
        float targetSlideSpeed;
        float SlideaccelRate = 3;
        // Calculate the desired slide speed 
        
        
            targetSlideSpeed = -3f; // slideSpeed
        
        
        // Remove the remaining upwards velocity to prevent upwards sliding and make sure y velocity dont excced target speed
        if (rb.velocity.y > 0 || rb.velocity.y < targetSlideSpeed)
        {
            rb.AddForce(-rb.velocity.y * Vector2.up, ForceMode2D.Impulse);
        }

        targetSlideSpeed = Mathf.Lerp(rb.velocity.y, targetSlideSpeed, 1);
        // Calculate the difference between the target slide speed and the current vertical velocity
        float speedDifference = targetSlideSpeed - rb.velocity.y;
        float slideMovement = speedDifference * SlideaccelRate;
        // Apply a force to reach the target slide speed
        //float force = slideMovement * rb.mass / Time.fixedDeltaTime; //use this when you want to reach targetSlideSpeed immediately
        //or you can change SlideaccelRate to really high
        rb.AddForce(Vector2.up * slideMovement, ForceMode2D.Force);
    }
    #endregion

    #region Ground/Wall Check
    private void GroundWallCheck() {
        if (!IsJumping)
        {
            //Ground Check
            if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer) && !IsJumping) //checks if set box overlaps with ground
            {
                LastOnGroundTime = CoyoteTime; //if so sets the lastGrounded to coyoteTime
            }
            //Right Wall Check
            if (((Physics2D.OverlapBox(frontWallCheck.position, wallCheckSize, 0, wallLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(backWallCheck.position, wallCheckSize, 0, wallLayer) && !IsFacingRight)) && !IsWallJumping)
                LastOnWallRightTime = CoyoteTime;

            //Right Wall Check
            if (((Physics2D.OverlapBox(frontWallCheck.position, wallCheckSize, 0, wallLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(backWallCheck.position, wallCheckSize, 0, wallLayer) && IsFacingRight)) && !IsWallJumping)
                LastOnWallLeftTime = CoyoteTime;
            //Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
    }
    #endregion

    #region Key Input
    void KeyInput()
    {
        xRawInput = Input.GetAxisRaw("Horizontal");
        yRawInput = Input.GetAxisRaw("Vertical");
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
    }
    #endregion

    #region Wall Jump
    private void WallJump(int dir)
    {
        //Ensures we can't call Wall Jump multiple times from one press
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        // Perform Wall Jump
        Vector2 force = new Vector2(15, 25);
        force.x *= dir; //apply force in opposite direction of wall

        if (Mathf.Sign(rb.velocity.x) != Mathf.Sign(force.x))
            force.x -= rb.velocity.x;

        if (rb.velocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
            force.y -= rb.velocity.y;

        //Unlike in the run we want to use the Impulse mode.
        //The default mode will apply are force instantly ignoring masss
        rb.AddForce(force, ForceMode2D.Impulse);


    }
    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
             (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }
    #endregion


    private void Flip()
    {
        if (IsFacingRight && xRawInput < 0 || !IsFacingRight && xRawInput > 0) {
            IsFacingRight = !IsFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;     
        }
        //stores scale and flips the player along the x axis, 
        
    }
    
}

