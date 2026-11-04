using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Colócalo en el objeto "recompensa" de cada nivel (donde el jugador
/// encuentra una habilidad nueva). Al tocarlo: desbloquea la habilidad
/// indicada, muestra el panel de aviso con su nombre/ícono/descripción, y
/// opcionalmente activa un portal (por ejemplo, el regreso al hub, si
/// estaba desactivado hasta este punto).
/// Con "Entregar Al Iniciar" la habilidad se da sola al cargar la escena
/// (nivel 3: la basura se tiene desde el principio).
/// </summary>
[RequireComponent(typeof(Collider))]
public class RecompensaHabilidad : MonoBehaviour
{
    [Header("Habilidad a desbloquear")]
    [Tooltip("0 = Cortar maleza, 1 = Regar, 2 = Recolectar basura")]
    [SerializeField] private int indiceHabilidad;

    [Header("Entrega automática (opcional)")]
    [Tooltip("Si está marcado, la habilidad se entrega sola al cargar la escena, sin tocar el objeto.")]
    [SerializeField] private bool entregarAlIniciar = false;

    [Tooltip("Segundos (tiempo real) que se espera tras cargar la escena antes de entregarla y mostrar el panel.")]
    [SerializeField, Min(0f)] private float retrasoAlIniciar = 1f;

    [Header("Panel de aviso")]
    [SerializeField] private PanelHabilidadDesbloqueada panelDesbloqueo;

    [Tooltip("Si el jugador ya tenía la habilidad (repite el nivel), no vuelve a mostrar el panel.")]
    [SerializeField] private bool omitirPanelSiYaLaTenia = true;

    [Header("Portal opcional a desbloquear")]
    [Tooltip("Objeto del portal que se activa al recoger esto (ej. el regreso " +
             "al hub, si empieza desactivado). Déjalo vacío si no aplica.")]
    [SerializeField] private GameObject portalDesbloqueado;

    [Header("Eventos")]
    [Tooltip("Se invoca al entregar la habilidad. Ej. nivel 2: ObjetivoCharcosLimpiados.Activar() " +
             "y ocultar el contador de enemigos.")]
    public UnityEvent AlEntregar = new UnityEvent();

    private bool yaActivado = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        if (entregarAlIniciar) StartCoroutine(EntregarConRetraso());
    }

    private IEnumerator EntregarConRetraso()
    {
        yield return new WaitForSecondsRealtime(retrasoAlIniciar);
        Entregar();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Entregar();
    }

    /// <summary>Desbloquea la habilidad, muestra el panel y oculta este objeto.</summary>
    public void Entregar()
    {
        if (yaActivado) return;

        // Se busca al jugador persistente por código, no por un campo
        // arrastrado en el Inspector: este objeto vive en la escena del
        // nivel, pero el jugador no existe ahí hasta que entra en Play.
        JugadorPersistente jugador = JugadorPersistente.Obtener();
        if (jugador == null || jugador.Habilidades == null) return;

        yaActivado = true;

        bool yaLaTenia = jugador.Habilidades.EstaDesbloqueada(indiceHabilidad);
        jugador.Habilidades.DesbloquearHabilidad(indiceHabilidad);
        Habilidad habilidad = jugador.Habilidades.ObtenerHabilidad(indiceHabilidad);

        bool mostrarPanel = !(yaLaTenia && omitirPanelSiYaLaTenia);
        if (mostrarPanel && panelDesbloqueo != null && habilidad != null)
            panelDesbloqueo.Mostrar(habilidad);

        if (portalDesbloqueado != null)
            portalDesbloqueado.SetActive(true);

        AlEntregar?.Invoke();
        gameObject.SetActive(false);
    }
}
