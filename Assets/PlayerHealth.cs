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

    void Start()
    {
        currentHealth = maxHealth;

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
        Debug.Log($" Collision joueur avec: {collision.gameObject.name} - Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log(" Ennemi détecté par le joueur!");
            TakeDamage(10);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
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
                Debug.Log(" Joueur mort!");
                // Ici vous pouvez ajouter la logique de game over
            }
        }
        else
        {
            Debug.Log(" Dégâts évités - Joueur invincible");
        }
    }

    public void Heal(int healAmount)
    {
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
        while (isInvincible)
        {
            if (graphics != null)
            {
                graphics.color = new Color(1f, 1f, 1f, 0.3f); // Semi-transparent
                yield return new WaitForSeconds(invincibilityFlashDelay);
                graphics.color = new Color(1f, 1f, 1f, 1f); // Normal
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
            graphics.color = new Color(1f, 1f, 1f, 1f); // S'assurer que c'est normal à la fin
        }
        Debug.Log(" Invincibilité terminée");
    }
}