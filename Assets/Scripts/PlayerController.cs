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

    public int currentHealth = 10;

    public bool jumped = false;
    public bool isDashing = false;

    public float dashSpeed = 10f;
    public float dashDist = 5f;
    public float dashTime = 3f;

    float dash;
    public float dashVelocity;

    public float terminalSpeed = -5f;

    public enum FacingDirection
    {
        left, right
    }

    public enum CharacterState
    {
        idle, walk, jump, die
    }

    public CharacterState currentState = CharacterState.idle;
    public CharacterState prevState = CharacterState.idle;

    public FacingDirection currentDirection = FacingDirection.left;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        gravity = (-2 * apexHeight) / (Mathf.Pow(apexTime, 2));
        jump = (2 * apexHeight) / apexTime;
        dash = (2 * dashSpeed) / dashTime;

        acceleration = maxSpeed / accelerationTime;

        coyoteTime = maxCoyoteTime;
    }

    void Update()
    {
        prevState = currentState;

        if (IsDead())
        {
            currentState = CharacterState.die;
        }
        
        switch (currentState)
        {
            case CharacterState.idle:
                if (IsWalking())
                {
                    currentState = CharacterState.walk;
                }
                if (!IsGrounded())
                {
                    currentState = CharacterState.jump;
                }
                break;
            case CharacterState.walk:
                if (!IsWalking())
                {
                    currentState = CharacterState.idle;
                }
                if (!IsGrounded())
                {
                    currentState = CharacterState.jump;
                }
                break;
            case CharacterState.jump:
                if (IsGrounded())
                {
                    if (IsWalking())
                    {
                        currentState = CharacterState.walk;
                    }
                    else
                    {
                        currentState = CharacterState.idle;
                    }
                }
                break;
            case CharacterState.die:
                break;

        }

        if ((Input.GetKeyDown(KeyCode.Space) && (IsGrounded() || coyoteTime > 0)) && !jumped)
        {
            jumped = true;
            jumpVelocity = jump;
        }

        if (Input.GetKeyDown(KeyCode.E) && !isDashing)
        {
            isDashing = true;
            dashVelocity = dash;

        }

        if (coyoteTime <= 0)
        {
            coyoteTime = 0;
        }

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

        if (isDashing)
        {
            dashVelocity -= dash * Time.deltaTime;
            rb.velocity = new Vector2(velocity.x + dashVelocity, rb.velocity.y);

            if (dashVelocity <= 0)
            {
                isDashing = false;
            }


        }

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
            // applies gravity when player is no longer grounded
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + gravity * Time.deltaTime, terminalSpeed);
        }

        else
        {
            if (IsGrounded() && rb.velocity.y < 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public void OnDeathAnimationComplete()
    {
        gameObject.SetActive(false);
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