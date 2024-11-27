using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float accelerationTime = 2f;
    float currentSpeed = 0f;
    public float maxSpeed;
    float acceleration;
    Vector3 velocity;

    public float apexHeight = 2f;
    public float apexTime = 3f;
    public float coyoteTime;
    public float maxCoyoteTime = 1f;

    float jump;
    float gravity;

    public float jumpVelocity;

    Rigidbody2D rb;

    public LayerMask ground;
    public Transform player;

    public bool jumped = false;

    public float terminalSpeed = -5f;

    public enum FacingDirection
    {
        left, right
    }

    public FacingDirection currentDirection = FacingDirection.left;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        gravity = (-2 * apexHeight) / (Mathf.Pow(apexTime, 2));
        jump = (2 * apexHeight) / apexTime;

        acceleration = maxSpeed / accelerationTime;

        coyoteTime = maxCoyoteTime;
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) && (IsGrounded() || coyoteTime > 0)) && !jumped)
        {
            jumped = true;
            jumpVelocity = jump;
        }

        if (coyoteTime <= 0)
        {
            coyoteTime = 0;
        }

        Debug.Log(IsGrounded());
    }

    void FixedUpdate()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        Vector2 playerInput = new Vector2(Input.GetAxis("Horizontal"), 0);
        MovementUpdate(playerInput);

    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.magnitude > 0)
        {
            currentSpeed += acceleration * Time.deltaTime;

            if (currentSpeed > maxSpeed)
            {
                currentSpeed = maxSpeed;
            }

            velocity = playerInput * currentSpeed;
        }

        else

        {
            currentSpeed -= acceleration * Time.deltaTime;

            if (currentSpeed < 0)
            {
                currentSpeed = 0;
            }

            velocity = Vector2.zero;
        }

        rb.velocity = new Vector2(velocity.x, rb.velocity.y);

        if (!IsGrounded())
        {
            coyoteTime -= Time.deltaTime;

        }
        else
        {
            coyoteTime = maxCoyoteTime;
        }


        if (jumped)
        {
            jumpVelocity += gravity * Time.deltaTime;
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);

            if (jumpVelocity < terminalSpeed)
            {
                jumpVelocity = terminalSpeed;
            }

            // when jump velocity reaches its max point, jump velocity increases, and then retracts because gravity subtracts it
            // the reason why it has to be ZERO is because the jump velocity first gets added from the jump formula, and then
            // gravity will also be applied at the same time, and once they meet the highest point, the jump velocity will become equal to or less than 0
            // that is when the jumped boolean should be set to false (it is technically no longer jumping, and gravity is just doing the rest)

            if (jumpVelocity <= 0)
            {
                jumped = false;
            }
        }

        else if (!IsGrounded())
        {
            jumpVelocity += gravity * Time.deltaTime;
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }

    }

    public bool IsWalking()
    {
        if (currentSpeed > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public bool IsGrounded()
    {
        Vector2 direction = new Vector2(1, -1);

        bool checkGrounded = Physics2D.Raycast(player.position, Vector2.down, 0.65f, ground);
        Debug.DrawRay(player.position, Vector2.down * 0.65f, Color.green);

        return checkGrounded;
    }

    public FacingDirection GetFacingDirection()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput > 0)
        {
            return currentDirection = FacingDirection.right;
        }
        else if (horizontalInput < 0)
        {
            return currentDirection = FacingDirection.left;
        }
        return currentDirection;
    }

}