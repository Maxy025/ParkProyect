using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Evento que se dispara cada vez que la vida cambia.
/// Parámetros: (vidaActual, vidaMaxima)
/// </summary>
[System.Serializable]
public class HealthChangedEvent : UnityEvent<float, float> { }

/// <summary>
/// Sistema de vida del jugador (o cualquier entidad).
/// - Recibe daño mediante TakeDamage()
/// - Se regenera automáticamente tras un tiempo sin recibir daño
/// - Emite eventos para que la UI (u otros sistemas) reaccionen
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Regeneración automática")]
    [Tooltip("Segundos que deben pasar sin recibir daño antes de empezar a regenerar")]
    [SerializeField] private float regenDelay = 3f;
    [Tooltip("Cantidad de vida regenerada por segundo")]
    [SerializeField] private float regenRate = 10f;
    [SerializeField] private bool canRegenerate = true;

    [Header("Eventos")]
    public HealthChangedEvent OnHealthChanged;
    public UnityEvent OnDamageTaken;
    public UnityEvent OnPlayerDeath;

    private float lastDamageTime = -Mathf.Infinity;
    private bool isDead = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Notifica a la UI el estado inicial (barra llena)
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (isDead) return;

        bool puedeEmpezarARegenerar = Time.time >= lastDamageTime + regenDelay;

        if (canRegenerate && currentHealth < maxHealth && puedeEmpezarARegenerar)
        {
            RegenerateHealth();
        }
    }

    /// <summary>
    /// Aplica daño a la entidad. Llama a esto desde el enemigo, trampa, etc.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        lastDamageTime = Time.time; // reinicia el contador de regeneración

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Cura vida instantáneamente (pociones, items, etc.)
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void RegenerateHealth()
    {
        currentHealth = Mathf.Clamp(currentHealth + regenRate * Time.deltaTime, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPlayerDeath?.Invoke();
        Debug.Log($"{gameObject.name} ha muerto.");
    }

    /// <summary>
    /// Revive o reinicia la vida (útil para respawn o checkpoints)
    /// </summary>
    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
