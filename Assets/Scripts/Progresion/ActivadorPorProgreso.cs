using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Activa o desactiva objetos según el progreso del juego.
/// Ejemplos en el Hub:
/// - Nivel1_Completado → enciende la zona del camellón restaurada y apaga la versión seca.
/// - Nivel3_Completado → enciende el punto de los créditos.
///
/// Revisa el progreso al cargar la escena y también en tiempo real
/// (por si el objetivo se completa mientras el jugador está en la escena).
///
/// IMPORTANTE: pon este componente en un objeto que siempre esté activo
/// (por ejemplo, un objeto vacío "Progresion"), no en uno de los objetos que controla.
/// </summary>
public class ActivadorPorProgreso : MonoBehaviour
{
    [Header("Condición")]
    [Tooltip("Ids que deben estar completados (TODOS). Ej: Nivel1_Completado, Nivel2_Completado, Nivel3_Completado, Hub_Portal1.")]
    [SerializeField] private string[] idsRequeridos = { ProgresoJuego.Nivel1 };

    [Header("Al cumplirse")]
    [Tooltip("Objetos que se ACTIVAN cuando se cumple la condición (y se ocultan mientras no se cumple).")]
    [SerializeField] private GameObject[] objetosAActivar;

    [Tooltip("Objetos que se DESACTIVAN cuando se cumple la condición (y se muestran mientras no se cumple).")]
    [SerializeField] private GameObject[] objetosADesactivar;

    [Header("Eventos")]
    [Tooltip("Se invoca si la condición se cumple MIENTRAS el jugador está en la escena (no al cargarla ya cumplida). Útil para sonidos o mensajes.")]
    public UnityEvent AlCumplirse;

    private bool cumplido;

    public bool Cumplido => cumplido;

    private void Start()
    {
        cumplido = ProgresoJuego.TodosCompletados(idsRequeridos);
        Aplicar(cumplido);

        ProgresoJuego.AlCompletarObjetivo += RevisarCondicion;
    }

    private void OnDestroy()
    {
        ProgresoJuego.AlCompletarObjetivo -= RevisarCondicion;
    }

    private void RevisarCondicion(string idCompletado)
    {
        if (cumplido) return;
        if (!ProgresoJuego.TodosCompletados(idsRequeridos)) return;

        cumplido = true;
        Aplicar(true);
        AlCumplirse?.Invoke();
    }

    private void Aplicar(bool condicionCumplida)
    {
        if (objetosAActivar != null)
            foreach (GameObject objeto in objetosAActivar)
                if (objeto != null) objeto.SetActive(condicionCumplida);

        if (objetosADesactivar != null)
            foreach (GameObject objeto in objetosADesactivar)
                if (objeto != null) objeto.SetActive(!condicionCumplida);
    }
}
