using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Basura recolectable en el mundo. El jugador la recoge automáticamente
/// al tocarla, siempre que tenga la habilidad de recolección equipada y
/// espacio disponible en su inventario.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ObjetoBasura : MonoBehaviour
{
    [SerializeField] private GameObject prefabEfectoRecoleccion;

    [Tooltip("Se invoca justo antes de que la basura desaparezca al ser recolectada. " +
             "Lo usa EstadoPersistente para que no reaparezca al volver a la escena.")]
    public UnityEvent AlRecolectar;

    private HabilidadRecolectarBasura habilidadBasura;

    private void Awake()
    {
        // Busca la habilidad automáticamente; en un proyecto con múltiples
        // jugadores conviene asignarla manualmente en su lugar.
        habilidadBasura = FindObjectOfType<HabilidadRecolectarBasura>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (habilidadBasura == null) return;

        if (habilidadBasura.IntentarRecolectar())
        {
            if (prefabEfectoRecoleccion != null)
                Instantiate(prefabEfectoRecoleccion, transform.position, Quaternion.identity);

            AlRecolectar?.Invoke();
            Destroy(gameObject);
        }
        // Si el inventario está lleno (o la habilidad no está equipada),
        // la basura simplemente permanece en el mundo.
    }
}
