using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Zona peligrosa que daña al jugador al contacto (fuego, ácido, espinas, etc.)
/// Se puede "limpiar" regándola con la habilidad de agua, dejándola segura
/// de forma permanente.
/// </summary>
public class ZonaPeligrosa : MonoBehaviour
{
    [SerializeField] private float cantidadDano = 10f;
    [SerializeField] private float enfriamientoDano = 1f;
    [Tooltip("Objeto visual del peligro (fuego, partículas, etc.) que se apaga al limpiar")]
    [SerializeField] private GameObject visualPeligro;
    [SerializeField] private Collider colisionadorPeligro;

    public UnityEvent AlLimpiar;

    private bool estaLimpia = false;
    private float ultimoTiempoDano = -Mathf.Infinity;

    private void OnTriggerStay(Collider other)
    {
        if (estaLimpia) return;
        if (Time.time < ultimoTiempoDano + enfriamientoDano) return;

        if (other.TryGetComponent(out PlayerHealth vida))
        {
            vida.TakeDamage(cantidadDano);
            ultimoTiempoDano = Time.time;
        }
    }

    public void LimpiarPeligro()
    {
        if (estaLimpia) return;
        estaLimpia = true;

        if (visualPeligro != null) visualPeligro.SetActive(false);
        if (colisionadorPeligro != null) colisionadorPeligro.enabled = false;

        AlLimpiar?.Invoke();
    }
}