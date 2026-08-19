using UnityEngine;

/// <summary>
/// Coloca este script en cualquier enemigo o trampa para que dañe al
/// jugador al hacer contacto. Funciona tanto en 2D como en 3D
/// (usa el que corresponda a tu Collider; los otros simplemente no se disparan).
/// </summary>
public class EnemyDamageDealer : MonoBehaviour
{
    [SerializeField] private float damageAmount = 10f;
    [Tooltip("Tiempo mínimo entre golpes, para evitar daño cada frame durante el contacto")]
    [SerializeField] private float damageCooldown = 1f;

    private float lastDamageTime = -Mathf.Infinity;

    // --- 2D ---
    private void OnTriggerEnter2D(Collider2D other) => TryDamage(other.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => TryDamage(collision.gameObject);

    // --- 3D ---
    private void OnTriggerEnter(Collider other) => TryDamage(other.gameObject);
    private void OnCollisionEnter(Collision collision) => TryDamage(collision.gameObject);

    private void TryDamage(GameObject other)
    {
        if (Time.time < lastDamageTime + damageCooldown) return;

        if (other.TryGetComponent(out PlayerHealth health))
        {
            health.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
        }
    }
}
