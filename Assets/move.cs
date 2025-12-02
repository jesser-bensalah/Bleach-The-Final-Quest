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

    // Variables pour l'attaque
    public bool isAttacking = false;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public float attackDuration = 0.8f;

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

        // Attaque avec Z
        if (Input.GetKeyDown(KeyCode.Z) && !isAttacking)
        {
            StartCoroutine(Attack());
        }

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

    // Coroutine d'attaque
    IEnumerator Attack()
    {
        isAttacking = true;
        anim.SetBool("IsAttacking", true);

        Debug.Log("Hero Fight declenchee!");

        // Détecter et tuer les ennemis avec délai
        KillEnemiesWithDeathDelay();

        // Durée de l'animation d'attaque
        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
        anim.SetBool("IsAttacking", false);
        Debug.Log("Attaque terminee");
    }

    // Tuer les ennemis avec un délai de mort
    void KillEnemiesWithDeathDelay()
    {
        // Déterminer la direction de l'attaque
        Vector2 attackDirection = spr.flipX ? Vector2.left : Vector2.right;
        Vector2 attackOrigin = (Vector2)transform.position + attackDirection * 0.8f;

        // Debug visuel
        Debug.DrawRay(attackOrigin, attackDirection * attackRange, Color.red, 1f);

        // OverlapCircle pour une zone plus large
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackOrigin, attackRange, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("Ennemi tue: " + enemy.name);

                // Lancer la séquence de mort avec délai
                StartCoroutine(EnemyDeathSequence(enemy.gameObject));
            }
        }

        if (enemies.Length == 0)
        {
            Debug.Log("Aucun ennemi dans la range");
        }
    }

    // Séquence de mort de l'ennemi avec délai avant disparition
    IEnumerator EnemyDeathSequence(GameObject enemy)
    {
        if (enemy != null)
        {
            // Désactiver le comportement de l'ennemi immédiatement
            EnemyPatrolWithWait patrolScript = enemy.GetComponent<EnemyPatrolWithWait>();
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            SpriteRenderer enemySprite = enemy.GetComponent<SpriteRenderer>();

            if (patrolScript != null)
                patrolScript.enabled = false;

            // Optionnel: Animation de mort ou effet visuel
            if (enemySprite != null)
            {
                // Changer la couleur pour indiquer la mort
                enemySprite.color = Color.red;

                // Attendre un délai avant de faire disparaître
                yield return new WaitForSeconds(3f); // Délai de mort

                // Faire disparaître progressivement
                float fadeTime = 0.5f;
                float elapsedTime = 0f;
                Color originalColor = enemySprite.color;

                while (elapsedTime < fadeTime)
                {
                    if (enemySprite != null)
                    {
                        float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                        enemySprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }
                }
            }
            else
            {
                // Si pas de SpriteRenderer, attendre simplement le délai
                yield return new WaitForSeconds(2f);
            }

            // Désactiver définitivement l'ennemi après le délai
            if (enemy != null)
            {
                enemy.SetActive(false);
                Debug.Log("Ennemi definitivement elimine: " + enemy.name);
            }
        }
    }

    private void FixedUpdate()
    {
        // VOTRE SYSTÈME DE SAUT ORIGINAL
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

        // Animation de course
        float characterVelocity = Mathf.Abs(rb.velocity.x);
        anim.SetFloat("Speed", characterVelocity);
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

    // Visualisation dans l'éditeur
    private void OnDrawGizmosSelected()
    {
        // Attack Range
        if (spr != null)
        {
            Vector2 attackDirection = spr.flipX ? Vector2.left : Vector2.right;
            Vector2 attackOrigin = (Vector2)transform.position + attackDirection * 0.8f;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackOrigin, attackRange);
        }
    }
}