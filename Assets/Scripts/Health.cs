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

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        onTakeDamage?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        onDeath?.Invoke();
        // For basic enemies, just destroy the object. 
        // For the player, you might trigger a game over screen.
        Destroy(gameObject); 
    }
}