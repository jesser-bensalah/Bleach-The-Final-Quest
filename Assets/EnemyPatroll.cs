using System.Collections;
using UnityEngine;

public class EnemyPatrolWithWait : MonoBehaviour
{
    public float speed = 2f;
    public Transform waypoint1;
    public Transform waypoint2;
    public float waitTime = 2f;
    public int damageOnCollision = 20;

    private SpriteRenderer spr;
    private bool movingToWaypoint2 = true;
    private bool isPatrolling = true;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();

        // Créer des waypoints par défaut si non assignés
        if (waypoint1 == null || waypoint2 == null)
        {
            CreateDefaultWaypoints();
        }

        if (waypoint1 != null && waypoint2 != null)
        {
            StartCoroutine(PatrolRoutine());
        }
        else
        {
            Debug.LogError("Waypoints are still null after creation!");
        }
    }

    IEnumerator PatrolRoutine()
    {
        while (isPatrolling)
        {
            if (waypoint1 == null || waypoint2 == null) yield break;

            Transform currentTarget = movingToWaypoint2 ? waypoint2 : waypoint1;
            if (currentTarget == null) yield break;

            // Flip selon la direction
            if (spr != null)
            {
                spr.flipX = (currentTarget.position.x < transform.position.x);
            }

            // Se déplacer vers la cible
            while (Vector3.Distance(transform.position, currentTarget.position) > 0.3f)
            {
                if (currentTarget == null) yield break;

                Vector3 direction = (currentTarget.position - transform.position).normalized;
                direction.y = 0;
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
            movingToWaypoint2 = !movingToWaypoint2;
        }
    }

    void CreateDefaultWaypoints()
    {
        Vector3 pos = transform.position;

        if (waypoint1 == null)
        {
            GameObject wp1 = new GameObject("Waypoint1");
            wp1.transform.position = new Vector3(pos.x - 3f, pos.y, pos.z);
            waypoint1 = wp1.transform;
        }

        if (waypoint2 == null)
        {
            GameObject wp2 = new GameObject("Waypoint2");
            wp2.transform.position = new Vector3(pos.x + 3f, pos.y, pos.z);
            waypoint2 = wp2.transform;
        }
    }

    // DÉTECTION DE COLLISION SUR L'ENNEMI
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($" Collision ennemi avec: {collision.gameObject.name} - Tag: {collision.gameObject.tag}");

        if (collision.transform.CompareTag("Player"))
        {
            Debug.Log("Joueur touché par l'ennemi!");

            PlayerHealth playerHealth = collision.transform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log($" Inflige {damageOnCollision} dégâts");
                playerHealth.TakeDamage(damageOnCollision);
            }
            else
            {
                Debug.LogError(" PlayerHealth non trouvé sur le joueur!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoint1 != null && waypoint2 != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(waypoint1.position, 0.3f);
            Gizmos.DrawWireSphere(waypoint2.position, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(waypoint1.position, waypoint2.position);
        }
    }
}