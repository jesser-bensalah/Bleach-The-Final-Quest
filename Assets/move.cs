using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    public float speed, jumpForce;
    float Hmove;
    Rigidbody2D rb;
    SpriteRenderer spr;
    Animator anim;
    bool jumping, grounded;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        jumping = false;
        grounded = false;
    }

    // Update is called once per frame
    void Update()
    {
        Hmove = Input.GetAxis("Horizontal");
        FlipPerso();

        // Saut avec flèche du haut
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            GoJump();
        }

        // Alternative: Saut avec W
        if (Input.GetKeyDown(KeyCode.W))
        {
            GoJump();
        }
    }

    public void GoJump()
    {
        if (grounded)
        {
            jumping = true;
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(Hmove * speed, rb.velocity.y);

        // Vérification améliorée du sol
        if (Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            grounded = true;
        }

        if (jumping)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumping = false;
            grounded = false;
        }

        // CORRECTION: Utiliser la valeur absolue pour l'animation
        float characterVelocity = Mathf.Abs(rb.velocity.x);
        anim.SetFloat("Speed", characterVelocity); // CORRECTION ICI
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
}