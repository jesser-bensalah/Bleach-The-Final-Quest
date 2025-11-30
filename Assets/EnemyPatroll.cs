using System.Collections;
using UnityEngine;

public class EnemyPatrolWithWait : MonoBehaviour
{
    public float speed = 2f;
    public Transform waypoint1;
    public Transform waypoint2;
    public float waitTime = 2f;

    private SpriteRenderer spr;
    private bool movingToWaypoint2 = true;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();

        // Créer des waypoints par défaut si non assignés
        if (waypoint1 == null || waypoint2 == null)
        {
            CreateDefaultWaypoints();
        }

        StartCoroutine(PatrolRoutine());
    }

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            // Déterminer la cible actuelle
            Transform currentTarget = movingToWaypoint2 ? waypoint2 : waypoint1;

            // Flip selon la direction
            spr.flipX = (currentTarget.position.x < transform.position.x);

            // Se déplacer vers la cible
            while (Vector3.Distance(transform.position, currentTarget.position) > 0.3f)
            {
                Vector3 direction = (currentTarget.position - transform.position).normalized;
                direction.y = 0; // Bloquer Y
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
                yield return null;
            }

            // Attendre au waypoint
            yield return new WaitForSeconds(waitTime);

            // Changer de direction
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