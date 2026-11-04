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

    [Tooltip("Sonido al depositar la basura en el bote.")]
    [SerializeField] private SonidoAleatorio sonidoDepositar = new SonidoAleatorio();

    /// <summary>
    /// Se invoca al depositar basura en CUALQUIER bote (envía la cantidad de esa entrega).
    /// Como el bote se crea en tiempo de ejecución, los objetivos de nivel se suscriben aquí
    /// (ej. ObjetivoBasuraRecolectada en modo AlDepositar).
    /// </summary>
    public static event System.Action<int> AlDepositarEnCualquierBote;

    private HabilidadRecolectarBasura habilidad;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void LimpiarEventoEstatico()
    {
        AlDepositarEnCualquierBote = null;
    }

    public void Inicializar(HabilidadRecolectarBasura habilidadPropietaria)
    {
        habilidad = habilidadPropietaria;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (habilidad == null || habilidad.BasuraCargada <= 0) return;

        int cantidad = habilidad.DepositarTodo();
        totalDepositado += cantidad;
        ReproductorAudio.ReproducirEn(sonidoDepositar, transform.position);
        AlDepositarBasura?.Invoke(totalDepositado);
        AlDepositarEnCualquierBote?.Invoke(cantidad);
    }
}
