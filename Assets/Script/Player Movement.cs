using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //overall
    private Rigidbody2D rb;
    //jump
    [SerializeField] private float jumpPower;
    [SerializeField] private float jumpTime;
    [SerializeField] private float jumpMultiplier;

    //ground check
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    //fall
    [SerializeField] float fallMultiplier;
    private Vector2 gravityVector;
    //
    bool isJumping;
    float jumpCounter;
    float yInput;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityVector = new Vector2(0, -Physics2D.gravity.y);
        float xInput = Input.GetAxisRaw("Horizontal");
         yInput = Input.GetAxisRaw("Vertical");
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Jump") && isGrounded())
        {
            rb.AddForce(new Vector2(rb.velocity.x, yInput*jumpPower),ForceMode2D.Impulse);
            isJumping = true;
            jumpCounter = 0;
        }
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
            if (rb.velocity.y > 0) {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.6f);
            }
        }
        if (rb.velocity.y > 0 && isJumping)
        {
            jumpCounter += Time.deltaTime;
            if (jumpCounter > jumpTime)
            {
                isJumping = false;
            }
            float t = jumpCounter / jumpTime;
            float currentJump = jumpMultiplier;
            if (t > 0.5f)
            {
                currentJump = jumpMultiplier * (0.1f);
            }


            rb.velocity += gravityVector * jumpMultiplier * Time.deltaTime;
        }
        if (rb.velocity.y < 0)
        {
            //higer gravity while falling
            //rb.gravityScale = 2 * fallMultiplier;
            //rb.velocity -= gravityVector * fallMultiplier * Time.deltaTime;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -50));

        }
        else {
            rb.gravityScale = 1;
        }
    }
    private bool isGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(0.4f, 0.3f), CapsuleDirection2D.Horizontal, 0, groundLayer);
    }
}
