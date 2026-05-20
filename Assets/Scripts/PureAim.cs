using UnityEngine;

public class PureAim : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Transform target;       // The player
    public Transform boneToRotate; // The chest or spine bone
    public Transform gunBarrel;    // The tip of the gun (ensure the Z-axis points forward)

    void LateUpdate()
    {
        if (target == null || boneToRotate == null || gunBarrel == null) return;

        // 1. Find the target point (aiming at chest height, not the floor)
        Vector3 targetPoint = target.position + Vector3.up * 1.5f;
        
        // 2. Find the mathematical line between the gun barrel and the target
        Vector3 directionToTarget = targetPoint - gunBarrel.position;

        // 3. Calculate the exact twist needed to align the barrel's current forward direction with the target line
        Quaternion correctiveTwist = Quaternion.FromToRotation(gunBarrel.forward, directionToTarget);

        // 4. Apply that twist to the chest bone
        boneToRotate.rotation = correctiveTwist * boneToRotate.rotation;
    }
}