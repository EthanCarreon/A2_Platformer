using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindPhysics : MonoBehaviour
{
    public GameObject player;
    Rigidbody2D rb;

    void Start()
    {
        rb = player.GetComponent<Rigidbody2D>();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject == player)
        {
            Debug.Log("Player in wind");
            rb.velocity = new Vector2(rb.velocity.x, 3f); 
        }
    }
}
