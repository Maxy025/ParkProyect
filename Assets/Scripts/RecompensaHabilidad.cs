using UnityEngine;

/// <summary>
/// Colócalo en el objeto "recompensa" de cada nivel (donde el jugador
/// encuentra una habilidad nueva). Al tocarlo: desbloquea la habilidad
/// indicada, muestra el panel de aviso con su nombre/ícono/descripción, y
/// opcionalmente activa un portal (por ejemplo, el regreso al hub, si
/// estaba desactivado hasta este punto).
/// </summary>
[RequireComponent(typeof(Collider))]
public class RecompensaHabilidad : MonoBehaviour
{
    [Header("Habilidad a desbloquear")]
    [Tooltip("0 = Cortar maleza, 1 = Regar, 2 = Recolectar basura")]
    [SerializeField] private int indiceHabilidad;

    [Header("Panel de aviso")]
    [SerializeField] private PanelHabilidadDesbloqueada panelDesbloqueo;

    [Header("Portal opcional a desbloquear")]
    [Tooltip("Objeto del portal que se activa al recoger esto (ej. el regreso " +
             "al hub, si empieza desactivado). Déjalo vacío si no aplica.")]
    [SerializeField] private GameObject portalDesbloqueado;

    private bool yaActivado = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaActivado) return;
        if (!other.CompareTag("Player")) return;

        // Se busca al jugador persistente por código, no por un campo
        // arrastrado en el Inspector: este objeto vive en la escena del
        // nivel, pero el jugador no existe ahí hasta que entra en Play.
        JugadorPersistente jugador = JugadorPersistente.Obtener();
        if (jugador == null || jugador.Habilidades == null) return;

        yaActivado = true;

        jugador.Habilidades.DesbloquearHabilidad(indiceHabilidad);
        Habilidad habilidad = jugador.Habilidades.ObtenerHabilidad(indiceHabilidad);

        if (panelDesbloqueo != null && habilidad != null)
            panelDesbloqueo.Mostrar(habilidad);

        if (portalDesbloqueado != null)
            portalDesbloqueado.SetActive(true);

        gameObject.SetActive(false);
    }
}