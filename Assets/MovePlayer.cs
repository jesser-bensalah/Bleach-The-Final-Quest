using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;
    public LayerMask groundLayer;

    float Hmove;
    Rigidbody2D rb;
    SpriteRenderer spr;
    bool jumping;
    bool grounded;

    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();

        // CONFIGURATION IMPORTANTE du Rigidbody2D
        if (rb != null)
        {
            rb.gravityScale = 3f; // Gravité plus forte
            rb.freezeRotation = true; // Empêche la rotation
        }

        // Créer le groundCheck s'il n'existe pas
        if (groundCheck == null)
        {
            GameObject check = new GameObject("GroundCheck");
            check.transform.SetParent(transform);
            check.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = check.transform;
        }
    }

    void Update()
    {
        Hmove = Input.GetAxisRaw("Horizontal");

        // Détection du sol
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            jumping = true;

        }

        FlipPerso();
    }

    private void FixedUpdate()
    {
        // Déplacement horizontal
        rb.velocity = new Vector2(Hmove * speed, rb.velocity.y);

        // Saut
        if (jumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumping = false;
        }
    }

    void FlipPerso()
    {
        if (Hmove < 0)
        {
            spr.flipX = true;
        }
        else if (Hmove > 0)
        {
            spr.flipX = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}