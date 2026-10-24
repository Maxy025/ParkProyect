using UnityEngine;

/// <summary>
/// Punto del Hub que abre los créditos al entrar el jugador.
/// Debe empezar oculto: un ActivadorPorProgreso con Nivel3_Completado lo enciende.
/// Si el jugador cierra los créditos, puede volver a verlos saliendo y entrando de nuevo.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ZonaCreditos : MonoBehaviour
{
    [Tooltip("PanelCreditos del Canvas del Hub.")]
    [SerializeField] private PanelCreditos panelCreditos;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (panelCreditos != null)
            panelCreditos.Mostrar();
        else
            Debug.LogWarning("[ZonaCreditos] Falta asignar el PanelCreditos.", this);
    }
}
