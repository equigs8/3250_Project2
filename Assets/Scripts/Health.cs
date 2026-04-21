using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onTakeDamage;

    [Header("Colliders")]
    public GameObject HitboxColliderRightLeg;
    
    public GameObject HitboxColliderLeftLeg;
    public GameObject HitboxColliderRightArm;
    public GameObject HitboxColliderLeftArm;
    public GameObject HitboxColliderHead;
    public GameObject HitboxColliderChest;

    [Header("Damage Multiplier")]
    public float LegDamageMultiplier;
    public float ArmDamageMultiplier;
    public float HeadDamageMultiplier;
    public float ChestDamageMultiplier;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        onTakeDamage?.Invoke(currentHealth);

        CheckIfAlive();
    }

    public void TakeDamageTo(float amount, string bodyPart)
    {
        currentHealth -= amount * GetDamageMultiplier(bodyPart);
        onTakeDamage?.Invoke(currentHealth);

        CheckIfAlive();
    }

    public void CheckIfAlive(){

        if (currentHealth <= 0)
        {
            Die();
        }
        
    }

    private float GetDamageMultiplier(string bodyPart)
    {
        switch (bodyPart)
        {
            case "Leg":
                return LegDamageMultiplier;
            case "Arm":
                return ArmDamageMultiplier;
            case "Head":
                return HeadDamageMultiplier;
            case "Chest":
                return ChestDamageMultiplier;
            default:
                return 1f;
        }
    }

    void Die()
    {
        onDeath?.Invoke();
        // For basic enemies, just destroy the object. 
        // For the player, trigger a game over screen.
        Destroy(gameObject); 
    }
}