using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    public float speed, jumpForce;
    float Hmove;
    public Rigidbody2D rb;
    public SpriteRenderer spr;
    public Animator anim;
    bool jumping, grounded;
    public Animator animator;

    // Variables pour l'attaque
    public bool isAttacking = false;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public float attackDuration = 0.8f;

    // Variable pour désactiver temporairement les collisions avec les ennemis
    private bool disableEnemyCollisions = false;

    public static move instance;
    public Collider2D playerCollider;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de PlayerMouvement dans la scène");
            return;
        }

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        jumping = false;
        grounded = false;
        disableEnemyCollisions = false;

        // Assignation automatique du collider si non assigné
        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider2D>();
            if (playerCollider == null)
            {
                // Chercher tous les colliders et prendre le premier actif
                Collider2D[] allColliders = GetComponents<Collider2D>();
                foreach (Collider2D col in allColliders)
                {
                    if (col.enabled)
                    {
                        playerCollider = col;
                        break;
                    }
                }

                if (playerCollider == null && allColliders.Length > 0)
                {
                    playerCollider = allColliders[0];
                }
            }

            if (playerCollider != null)
            {
                Debug.Log($"Collider assigné automatiquement: {playerCollider.GetType().Name}");
            }
            else
            {
                Debug.LogWarning("Aucun Collider2D trouvé sur le joueur!");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Hmove = Input.GetAxis("Horizontal");
        FlipPerso();

        // Attaque avec Z
        if (Input.GetKeyDown(KeyCode.Z) && !isAttacking && !PlayerHealth.instance.isDead)
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
        if (grounded && !PlayerHealth.instance.isDead)
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

    // Séquence de mort de l'ennemi avec délai avant disparition - CORRIGÉ
    IEnumerator EnemyDeathSequence(GameObject enemy)
    {
        if (enemy != null)
        {
            // Désactiver le comportement de l'ennemi immédiatement
            EnemyPatrolWithWait patrolScript = enemy.GetComponent<EnemyPatrolWithWait>();
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            SpriteRenderer enemySprite = enemy.GetComponent<SpriteRenderer>();

            // IMMÉDIATEMENT: Désactiver les dégâts de l'ennemi
            if (patrolScript != null)
            {
                patrolScript.enabled = false;

                // Désactiver le collider de dégâts si possible
                if (enemyCollider != null)
                {
                    enemyCollider.enabled = false; // Désactive les collisions
                }
            }

            // Optionnel: Animation de mort ou effet visuel
            if (enemySprite != null)
            {
                enemySprite.color = Color.red;

                // RÉDUIRE le délai de mort (de 6s à 0.5s)
                yield return new WaitForSeconds(0.5f); // CORRECTION ICI

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
                // Si pas de SpriteRenderer, attendre simplement le délai réduit
                yield return new WaitForSeconds(0.5f); // CORRECTION ICI
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
        // Ne pas bouger si mort
        if (PlayerHealth.instance.isDead)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

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

    // Méthode pour désactiver temporairement les collisions avec les ennemis (optionnel)
    public void DisableEnemyCollisions(float duration)
    {
        if (!disableEnemyCollisions)
        {
            StartCoroutine(DisableEnemyCollisionsCoroutine(duration));
        }
    }

    IEnumerator DisableEnemyCollisionsCoroutine(float duration)
    {
        disableEnemyCollisions = true;
        yield return new WaitForSeconds(duration);
        disableEnemyCollisions = false;
    }

    // Modifier la détection de collision pour ignorer les collisions pendant l'attaque
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignorer les collisions avec les ennemis si désactivé
        if (disableEnemyCollisions && collision.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider, true);
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