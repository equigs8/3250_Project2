using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform player;
    public float visionRange = 20f;
    public float visionAngle = 45f;
    public LayerMask obstacleMask;

    [Header("Combat Settings")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float fireRate = 1.0f;
    public float accuracyOffset = 0.1f; // Higher = less accurate

    private float nextFireTime;
    private bool playerInSight;

    void Update()
    {
        CheckLineOfSight();

        if (playerInSight)
        {
            // Rotate to look at player (only on Y axis for now)
            Vector3 targetDir = player.position - transform.position;
            targetDir.y = 0; 
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * 5f);

            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void CheckLineOfSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= visionRange)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle <= visionAngle)
            {
                // Check if anything blocks the view
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    playerInSight = true;
                    return;
                }
            }
        }
        playerInSight = false;
    }

    void Shoot()
    {
        // Calculate direction with random inaccuracy
        Vector3 shootDir = (player.position - shootPoint.position).normalized;
        shootDir += new Vector3(
            Random.Range(-accuracyOffset, accuracyOffset),
            Random.Range(-accuracyOffset, accuracyOffset),
            Random.Range(-accuracyOffset, accuracyOffset)
        );

        Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(shootDir));
    }
    // This draws the vision cone in the Scene View
    private void OnDrawGizmos()
    {
        // Set the color to green (not alert) or red (player in sight)
        Gizmos.color = playerInSight ? Color.red : Color.green;

        // Draw the vision range circle
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Calculate the boundary lines of the vision cone
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle, 0) * transform.forward;

        // Draw the lines representing the FOV angle
        Gizmos.DrawRay(transform.position + Vector3.up, leftBoundary * visionRange);
        Gizmos.DrawRay(transform.position + Vector3.up, rightBoundary * visionRange);
        
        // If the player exists, draw a line to them if they are being detected
        if (player != null && playerInSight)
        {
            Gizmos.DrawLine(transform.position + Vector3.up, player.position);
        }
    }
}