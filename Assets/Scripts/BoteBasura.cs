using UnityEngine;

/// <summary>
/// Bote de basura generado por HabilidadRecolectarBasura. El jugador deposita
/// toda la basura que lleva encima al tocarlo.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BoteBasura : MonoBehaviour
{
    [Tooltip("Total acumulado depositado en este bote (persiste mientras exista el objeto)")]
    [SerializeField] private int totalDepositado = 0;

    [Tooltip("Se invoca con el total depositado tras cada entrega. Útil para abrir " +
             "una puerta o zona nueva al alcanzar cierta cantidad de basura recolectada.")]
    public EventoEntero AlDepositarBasura;

    private HabilidadRecolectarBasura habilidad;

    public void Inicializar(HabilidadRecolectarBasura habilidadPropietaria)
    {
        habilidad = habilidadPropietaria;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (habilidad == null || habilidad.BasuraCargada <= 0) return;

        totalDepositado += habilidad.DepositarTodo();
        AlDepositarBasura?.Invoke(totalDepositado);
    }
}
