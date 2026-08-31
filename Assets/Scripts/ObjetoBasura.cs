using UnityEngine;

/// <summary>
/// Basura recolectable en el mundo. El jugador la recoge automáticamente
/// al tocarla, siempre que tenga la habilidad de recolección equipada y
/// espacio disponible en su inventario.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ObjetoBasura : MonoBehaviour
{
    [SerializeField] private GameObject prefabEfectoRecoleccion;

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

            Destroy(gameObject);
        }
        // Si el inventario está lleno (o la habilidad no está equipada),
        // la basura simplemente permanece en el mundo.
    }
}
