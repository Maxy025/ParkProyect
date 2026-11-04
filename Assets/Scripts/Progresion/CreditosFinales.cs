using System.Collections;
using UnityEngine;

/// <summary>
/// Va en el Hub. Al regresar del último nivel (Nivel3_Completado) muestra automáticamente
/// el PanelCreditos una sola vez por partida. Después el jugador puede seguir explorando
/// y volver a verlos en la ZonaCreditos.
/// </summary>
public class CreditosFinales : MonoBehaviour
{
    [Tooltip("PanelCreditos del Canvas del Hub.")]
    [SerializeField] private PanelCreditos panelCreditos;

    [Tooltip("Ids que deben estar completados para mostrar los créditos (por defecto, el nivel 3).")]
    [SerializeField] private string[] idsRequeridos = { ProgresoJuego.Nivel3 };

    [Tooltip("Id que se guarda al mostrarlos, para que no vuelvan a salir solos en la misma partida.")]
    [SerializeField] private string idCreditosVistos = "Creditos_Vistos";

    [Tooltip("Segundos (tiempo real) que se espera tras cargar el Hub antes de abrir los créditos, " +
             "para que el jugador ya esté colocado y la música del Hub haya empezado.")]
    [SerializeField, Min(0f)] private float retraso = 1.5f;

    private void Start()
    {
        if (!ProgresoJuego.TodosCompletados(idsRequeridos)) return;
        if (ProgresoJuego.EstaCompletado(idCreditosVistos)) return;

        StartCoroutine(MostrarConRetraso());
    }

    private IEnumerator MostrarConRetraso()
    {
        yield return new WaitForSecondsRealtime(retraso);

        if (panelCreditos == null)
        {
            Debug.LogWarning("[CreditosFinales] Falta asignar el PanelCreditos.", this);
            yield break;
        }

        ProgresoJuego.MarcarCompletado(idCreditosVistos);
        panelCreditos.Mostrar();
    }
}
