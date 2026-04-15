using UnityEngine;
using InfimaGames.LowPolyShooterPack; // Ensure you include the namespace

public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform player;
    public float visionRange = 20f;
    public float visionAngle = 45f;
    public LayerMask obstacleMask;

    [Header("Combat Settings")]
    // Reference the Weapon component instead of a projectile prefab
    public WeaponBehaviour activeWeapon; 
    public float fireRate = 0.5f; 
    [Range(0, 2)] public float accuracyOffset = 0.1f;

    private float nextFireTime;
    private bool playerInSight;

    void Start()
    {
        // If not assigned, try to find it in children
        if (activeWeapon == null)
            activeWeapon = GetComponentInChildren<WeaponBehaviour>();
    }

    void Update()
    {
        CheckLineOfSight();

        if (playerInSight)
        {
            LookAtPlayer();

            // The Weapon.cs script handles its own internal ROF, 
            // but we use this timer to control the AI's decision to pull the trigger.
            if (Time.time >= nextFireTime)
            {
                if (activeWeapon != null)
                {
                    // Use the Fire method from Weapon.cs
                    activeWeapon.Fire(accuracyOffset);
                    
                    // Automatically reload if empty
                    if (!activeWeapon.HasAmmunition())
                        activeWeapon.Reload();
                }
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void LookAtPlayer()
    {
        Vector3 targetDir = player.position - transform.position;
        targetDir.y = 0; 
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * 5f);
    }

    void CheckLineOfSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= visionRange)
        {
            if (Vector3.Angle(transform.forward, directionToPlayer) <= visionAngle)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    playerInSight = true;
                    return;
                }
            }
        }
        playerInSight = false;
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