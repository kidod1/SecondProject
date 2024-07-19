using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 lastValidPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastValidPosition = rb.position;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("MapBoundary"))
        {
            rb.position = lastValidPosition;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("MapBoundary"))
        {
            lastValidPosition = rb.position;
        }
    }
}
