using UnityEngine;

public class EnemyAimTracker : MonoBehaviour
{
    public Transform playerTransform; // Drag the player here
    public Transform aimTarget;       // The GameObject your Animation Rigging constraints are tracking

    void Update()
    {
        if (playerTransform != null && aimTarget != null)
        {
            // 1. Move the Rigging Target to the player's chest/head
            aimTarget.position = playerTransform.position + Vector3.up * 1.5f; 

            // 2. Rotate the Enemy's root body to face the player (The "Yaw")
            Vector3 lookDirection = playerTransform.position - transform.position;
            lookDirection.y = 0; // Keep the enemy standing straight up
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}