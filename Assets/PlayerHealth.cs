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

        Debug.Log($" Collision joueur avec: {collision.gameObject.name} - Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log(" Ennemi détecté par le joueur!");
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
            currentHealth -= damage;
            Debug.Log($" Santé: {currentHealth}/{maxHealth} (-{damage})");

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
            Debug.Log(" Dégâts évités - Joueur invincible ou mort");
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

        Debug.Log($" Soin: {currentHealth}/{maxHealth} (+{healAmount})");
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
        Debug.Log(" Invincibilité terminée");
    }

    // SUPPRIMER la première méthode Respawn() et garder celle-ci
    public void Respawn()
    {
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
            move.instance.rb.bodyType = RigidbodyType2D.Dynamic;

            // Réactiver le collider si nécessaire
            // move.instance.playerCollider.enabled = true;

            // Réinitialiser l'animator
            Animator playerAnimator = move.instance.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.Rebind();
                playerAnimator.Play("Idle");

                // Optionnel: Déclencher une animation de respawn si elle existe
                // playerAnimator.SetTrigger("Respawn");
            }
        }

        if (graphics != null)
        {
            graphics.color = new Color(1f, 1f, 1f, 1f);
        }
    }
}