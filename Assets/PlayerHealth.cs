using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Healthbar healthbar;
    public bool isInvincible = false;
    public SpriteRenderer graphics;
    public float invincibilityFlashDelay = 0.2f;
    public float invincibilityTimeAfterHit = 2f;
    public bool isDead = false;

    public AudioClip hitSound; 
    public static PlayerHealth instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de PlayerHealth dans la scène");
            return;
        }

        instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (healthbar != null)
        {
            healthbar.SetMaxHealth(maxHealth);
        }
        else
        {
            Debug.LogError("Healthbar reference is missing in PlayerHealth!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(20);
        }
    }

    // DÉTECTION DE COLLISION SUR LE JOUEUR
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        Debug.Log($"Collision joueur avec: {collision.gameObject.name} - Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Ennemi détecté par le joueur!");
            TakeDamage(10);
        }
    }

    public void HealPlayer(int amount)
    {
        if (isDead) return;

        if ((currentHealth + amount) > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += amount;
        }

        healthbar.SetHealth(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible && !isDead)
        {
            AudioManager.instance.PlayClipAt(hitSound, transform.position);
            currentHealth -= damage;
            Debug.Log($"Santé: {currentHealth}/{maxHealth} (-{damage})");

            // Activation de l'invincibilité
            isInvincible = true;
            StartCoroutine(InvincibilityFlash());
            StartCoroutine(HandleInvincibilityDelay());

            // Mise à jour de la healthbar
            if (healthbar != null)
            {
                healthbar.SetHealth(currentHealth);
            }

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
                return;
            }
        }
        else
        {
            Debug.Log("Dégâts évités - Joueur invincible ou mort");
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Le joueur est mort!");

        StopAllCoroutines();

        if (graphics != null)
        {
            graphics.color = new Color(1f, 1f, 1f, 1f);
        }

        // Désactiver le mouvement et déclencher l'animation
        if (move.instance != null)
        {
            move.instance.enabled = false;

            Animator playerAnimator = move.instance.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                bool hasDieParameter = false;
                foreach (AnimatorControllerParameter param in playerAnimator.parameters)
                {
                    if (param.name == "Die" && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        hasDieParameter = true;
                        break;
                    }
                }

                if (hasDieParameter)
                {
                    playerAnimator.SetTrigger("Die");
                    Debug.Log("Animation Die déclenchée via Animator de move");
                }
                else
                {
                    playerAnimator.Play("Héro die");
                    Debug.Log("Animation 'Héro die' jouée directement");
                }
            }
            else
            {
                Debug.LogError("Animator non trouvé sur le joueur!");
            }

            move.instance.rb.bodyType = RigidbodyType2D.Kinematic;
            move.instance.rb.velocity = Vector3.zero;

            GameOverManager.instance.OnPlayerDeath();
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (healthbar != null)
        {
            healthbar.SetHealth(currentHealth);
        }

        Debug.Log($"Soin: {currentHealth}/{maxHealth} (+{healAmount})");
    }

    public IEnumerator InvincibilityFlash()
    {
        while (isInvincible && !isDead)
        {
            if (graphics != null)
            {
                graphics.color = new Color(1f, 1f, 1f, 0.3f);
                yield return new WaitForSeconds(invincibilityFlashDelay);
                graphics.color = new Color(1f, 1f, 1f, 1f);
                yield return new WaitForSeconds(invincibilityFlashDelay);
            }
            else
            {
                yield return new WaitForSeconds(invincibilityFlashDelay * 2);
            }
        }
    }

    public IEnumerator HandleInvincibilityDelay()
    {
        yield return new WaitForSeconds(invincibilityTimeAfterHit);
        isInvincible = false;
        if (graphics != null)
        {
            graphics.color = new Color(1f, 1f, 1f, 1f);
        }
        Debug.Log("Invincibilité terminée");
    }

    // SUPPRIMER la première méthode Respawn() et garder celle-ci
    public void Respawn()
    {
        Debug.Log($"Respawn appelé. move.instance: {move.instance}, playerCollider: {move.instance?.playerCollider}");

        isDead = false;
        isInvincible = false;
        currentHealth = maxHealth;

        if (healthbar != null)
        {
            healthbar.SetHealth(currentHealth);
        }

        if (move.instance != null)
        {
            move.instance.enabled = true;

            // Réactiver le Rigidbody
            if (move.instance.rb != null)
            {
                move.instance.rb.bodyType = RigidbodyType2D.Dynamic;
                move.instance.rb.velocity = Vector2.zero;
            }

            // Réactiver le collider avec vérifications supplémentaires
            if (move.instance.playerCollider != null)
            {
                move.instance.playerCollider.enabled = true;
            }
            else
            {
                // Recherche plus approfondie du collider
                Collider2D foundCollider = move.instance.GetComponent<Collider2D>();
                if (foundCollider != null)
                {
                    foundCollider.enabled = true;
                    move.instance.playerCollider = foundCollider; // Mettre à jour la référence
                }
                else
                {
                    Debug.LogWarning("Impossible de trouver un collider sur le joueur lors du respawn");
                }
            }

            // Réinitialiser l'animator
            Animator playerAnimator = move.instance.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.Rebind();
                playerAnimator.Play("Idle");
                // Réinitialiser tous les paramètres
                playerAnimator.SetBool("IsAttacking", false);
                playerAnimator.SetFloat("Speed", 0f);
            }

            // Réactiver aussi le SpriteRenderer
            if (move.instance.spr != null)
            {
                move.instance.spr.enabled = true;
            }
        }
        else
        {
            Debug.LogError("move.instance est null lors du respawn!");
        }

        if (graphics != null)
        {
            graphics.color = new Color(1f, 1f, 1f, 1f);
        }

        Debug.Log("Respawn complet");
    }
}