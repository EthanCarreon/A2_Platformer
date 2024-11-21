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

    Rigidbody2D rb;

    public LayerMask ground;
    public Transform player;

    public enum FacingDirection
    {
        left, right
    }

    public FacingDirection currentDirection = FacingDirection.left;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        Vector2 playerInput = new Vector2(Input.GetAxis("Horizontal"), 0).normalized;
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        acceleration = maxSpeed / accelerationTime;

        if (playerInput.magnitude > 0)
        {
            currentSpeed += acceleration * Time.deltaTime;

            if (currentSpeed > maxSpeed)
            {
                currentSpeed = maxSpeed;
            }

            velocity = playerInput;
        }

        else

        {
            currentSpeed -= acceleration * Time.deltaTime;

            if (currentSpeed < 0)
            {
                currentSpeed = 0;
            }
        }

        transform.position += velocity * currentSpeed * Time.deltaTime;

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, 5f);
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
        bool checkGrounded = Physics2D.Raycast(player.position, Vector2.down, 1f, ground);
        Debug.DrawRay(player.position, Vector2.down * 1f, Color.green);

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
