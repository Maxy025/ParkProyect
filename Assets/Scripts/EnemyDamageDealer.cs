using UnityEngine;

public class EnemyDamageDealer : MonoBehaviour
{
    [SerializeField] private float damageAmount = 10f;
    [Tooltip("Tiempo mínimo entre golpes, para evitar daño cada frame durante el contacto")]
    [SerializeField] private float damageCooldown = 1f;

    private float lastDamageTime = -Mathf.Infinity;
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
