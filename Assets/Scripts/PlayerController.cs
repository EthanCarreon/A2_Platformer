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
    public float wallJumpVelocity;

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

    public bool onWall = false;
    public bool onWallAndJumped = false;

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

        // a separate boolean variable that checks if the character is touching the wall, and is not grounded,
        // this boolean's purpose is to check if the player is touching a wall, but is also not grounded, but this doesn't
        // exactly mean that the player has also made a wall jump yet

        //if (IsTouchingWall() && !IsGrounded())
        //{
        //    onWall = true;
        //}
        //else
        //{
        //    onWall = false;
        //}

        // animation switch cases depending on player input

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

        // conditional statement to check if the player has jumped off the ground, and also check if they have enough coyote time to make a forgiving jump

        if ((Input.GetKeyDown(KeyCode.Space) && (IsGrounded() || coyoteTime > 0)) && !jumped)
        {
            jumped = true;
            jumpVelocity = jump;
        }

        // conditional statemtn to check if the player 

        if (IsTouchingWall() && Input.GetKeyDown(KeyCode.Space) && !isDashing && !onWallAndJumped)
        {
            jumped = true;
            onWallAndJumped = true;
            wallJumpVelocity = jump;
        }

        if (Input.GetKeyDown(KeyCode.E) && !isDashing)
        {
            isDashing = true;
            dashVelocity = dash;

        }

        // if the coyote time ever reaches less than or equal to 0, keep the coyote time at 0 so it doesnt continue to subtract indefinitely.

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

        if (isDashing)
        {
            playerInput.x = 0;
        }

        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        // this checks if the player is touching a wall, if they are, set the player input for the x value to 0 so they cant move
        // the isTouchingWall method uses a raycast, so if the player turns around and the raycast flips, the player will technically no longer be touching the wall

        if (IsTouchingWall())
        {
                playerInput.x = 0; 
        }

        // horizontal movement
        
        // check if the player's magnitude is greater than 0, basically means it is checking if the player is moving
        if (playerInput.magnitude > 0)
        {
            // add acceleration formula to the current speed multiplied by time.deltatime
            currentSpeed += acceleration * Time.deltaTime;

            // if the current speed every reaches above the set max speed, make the current speed always equal to the max speed so it doesnt go above
            if (currentSpeed > maxSpeed)
            {
                currentSpeed = maxSpeed;
            }

            // set the velocity (which is initiated as a vector in the beginning) to equal the player input multiplied by the current speed
            // what will happen is the player input (which is just the horizontal input) will move according to the current speed
            // the velocity variable is uesd as a vector3, so that it can be applied to the rigidbodies constructor
            velocity = playerInput * currentSpeed;
        }

        else

        {
            // else if the player is not moving (or if there is no player input) start decelerating the speed
            // I use the acceleration variable, but I could also have a seperate deceleration variable if I wanted the accerlation and the deceleration times to be different
            // subtract from the current speed multiplied by time.deltatime
            currentSpeed -= acceleration * Time.deltaTime;

            // if the current speed ever reaches below 0, set the current speed to 0 so it doesnt go into the negatives
            if (currentSpeed < 0)
            {
                currentSpeed = 0;
            }

            // set the velocity to vector2.zero so it actually stops moving
            // If this line is not added, when the player decelerates it might not always stop moving and could be inconsistent

            velocity = Vector2.zero;
        }

        // finally, set the rigidbodies velocity x value to equal the velocity that was used to change the current speed from the player input

        rb.velocity = new Vector2(velocity.x, rb.velocity.y);

        // dashing mechanic

        if (isDashing)
        {
            // checking which direction the character is currently facing, depending on the direction,

            if (currentDirection == FacingDirection.right)
            {
                // initiate the rigidbodies velocity and add positive or negative force 
                // if the character is facing right, add positive velocity (so it goes right) vice versa, subtract so it goes left

                rb.velocity = new Vector2(rb.velocity.x + dashVelocity, rb.velocity.y);
                dashVelocity -= dash * Time.deltaTime;

                // also subtract the dash velocity over time so the dashing actually comes to a stop

                if (dashVelocity <= 0)
                {
                    isDashing = false;
                    dashVelocity = 0;
                }
            }

            // check if facing left
            else if (currentDirection == FacingDirection.left)
            {
                rb.velocity = new Vector2(rb.velocity.x - dashVelocity, rb.velocity.y);
                dashVelocity -= dash * Time.deltaTime;

                // also subtract the dash velocity over time so the dashing actually comes to a stop & also set the isDashing boolean so that it knows it is no longer dashing

                if (dashVelocity <= 0)
                {
                    isDashing = false;
                    dashVelocity = 0;
                }
            }

        }

        // coyote time, check if the player is NOT grounded, if it they are not, which means they have reached an edge or jumped,
        // subtract the coyote time by time.deltatime so the player has a limited time before the coyote time reaches 0.

        if (!IsGrounded())
        {
            coyoteTime -= Time.deltaTime;

        }
        // if they are grounded, then just set thet coyote time to the max coyote time.
        else
        {
            coyoteTime = maxCoyoteTime;
        }

        // wall jumping, checks using a boolean to check if the player is colliding with a wall AND has jumped or just isnt grounded

        if (onWallAndJumped)
        {
            // using a separate velocity variable, add itself with the gravity so that it can also be applied the
            // same gravity when the player jumps
            wallJumpVelocity += gravity * Time.deltaTime;
            // add the wall jump velocity to the rigidbodies velocity
            rb.velocity = new Vector2(rb.velocity.x, wallJumpVelocity);

            // it is NEGATIVE 5 here because I need to check when the player falls down, for a regular jump, it only checks if it is less than or equal to 0,
            // but when it reaches its 0 point, it has reached its apex height point. Here, what I need to do is check if the player has reached BELOW
            // their apex height, and if they have, then it can sent the wall jumped boolean to false
            // I put -5 as a random testing value and it seems that it works pretty well

            if (wallJumpVelocity <= -5)
            {
                onWallAndJumped = false;
            }
        }

        // jump mechanic

        // conditional statement to check if the jumped boolean variable is true

        if (jumped)
        {

            // if the player has jumped, set the jump velocity to add itself with the gravity that subtracts the jump velocity
            // it subtracts the jump velocity because it has a negative value in the formula, and then multiply itself by time.deltatime so it can run over time
            jumpVelocity += gravity * Time.deltaTime;
            // then, apply it to the rigidbody velocity by putting it in the y value in the vector2's constructor
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);

            // if the jump velocity ever reaches less than the terminal speed that is set in the inspector, set it to the terminal speed so it doesnt go below
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


    }

    // player input methods to check specific inputs coming from the player for animations

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

    // check if player is grounded method
    public bool IsGrounded()
    {
        // set a raycast that points downwards at a certain length that fits perfectly from the origin point of the player, and down to where it just about touches the ground
        bool checkGrounded = Physics2D.Raycast(player.position, Vector2.down, 0.65f, ground);
        // draw the ray green so I can see it through gizmos
        Debug.DrawRay(player.position, Vector2.down * 0.65f, Color.green);

        return checkGrounded;
    }

    public bool IsTouchingWall()
    {
        Vector2 direction;

        if (currentDirection == FacingDirection.right)
        {
            direction = Vector2.right;
        }
        else
        {
            direction = Vector2.left;
        }

        bool checkOnWall = Physics2D.Raycast(player.position, direction, 0.7f, ground);
        Debug.DrawRay(player.position, direction * 0.7f, Color.red);

        return checkOnWall;
    }

    public void WallJump()
    {

        jumped = true;
        jumpVelocity = jump;

        onWallAndJumped = false;  

        Debug.Log("Wall Jump performed!");
    }

    public FacingDirection GetFacingDirection()
    {
        float horizontalInput = Input.GetAxis("Horizontal");    

        if (!isDashing)
        {
            if (horizontalInput > 0)
            {
                return currentDirection = FacingDirection.right;
            }
            else if (horizontalInput < 0)
            {
                return currentDirection = FacingDirection.left;
            }
        }
        return currentDirection;
    }

}