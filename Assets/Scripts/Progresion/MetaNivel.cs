using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Línea de meta de un nivel. Al tocarla el jugador:
/// 1) se marca el nivel como completado en ProgresoJuego,
/// 2) la zona se ve restaurada (se apagan los objetos "deteriorados" y se encienden los "restaurados"),
/// 3) opcionalmente se desbloquea una habilidad y se muestra su panel,
/// 4) tras unos segundos el jugador regresa automáticamente al Hub.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MetaNivel : MonoBehaviour
{
    [Header("Nivel")]
    [Tooltip("Id que se guarda en ProgresoJuego al llegar a la meta. Usa Nivel1_Completado, Nivel2_Completado o Nivel3_Completado.")]
    [SerializeField] private string idNivelCompletado = ProgresoJuego.Nivel1;

    [Header("Restauración visual")]
    [Tooltip("Objetos del nivel descuidado (basura, pasto seco, grafiti...). Se DESACTIVAN al llegar a la meta.")]
    [SerializeField] private GameObject[] objetosDeteriorados;

    [Tooltip("Objetos del nivel restaurado (flores, árboles sanos, bancas nuevas...). Se ACTIVAN al llegar a la meta. Pueden estar desactivados en la escena.")]
    [SerializeField] private GameObject[] objetosRestaurados;

    [Tooltip("Si el nivel ya se había completado antes, al volver a entrar se muestra restaurado desde el inicio.")]
    [SerializeField] private bool mostrarRestauradoSiYaCompletado = true;

    [Tooltip("Oculta a los enemigos que sigan vivos en el nivel al llegar a la meta, para que no ataquen durante la espera.")]
    [SerializeField] private bool ocultarEnemigosAlLlegar = true;

    [Header("Recompensa (opcional)")]
    [Tooltip("-1 = ninguna (nivel 3) · 0 = Tijeras (nivel 1) · 1 = Regar (nivel 2) · 2 = Basura")]
    [SerializeField] private int indiceHabilidad = -1;

    [Tooltip("Panel que muestra la habilidad desbloqueada. El tiempo se congela mientras está abierto, así que la cuenta para regresar al Hub empieza cuando el jugador lo cierra.")]
    [SerializeField] private PanelHabilidadDesbloqueada panelDesbloqueo;

    [Header("Mensaje (opcional)")]
    [Tooltip("Objeto de UI que se enciende al llegar (ej. un texto '¡Camellón restaurado!'). Se oculta al iniciar.")]
    [SerializeField] private GameObject mensajeMeta;

    [Header("Regreso al Hub")]
    [Tooltip("Nombre EXACTO de la escena del Hub (debe estar en Build Settings).")]
    [SerializeField] private string nombreEscenaHub = "Hub";

    [Tooltip("Id del PuntoAparicion del Hub donde aparece el jugador al regresar.")]
    [SerializeField] private string idPuntoAparicionHub = "DesdeNivel1";

    [Tooltip("Segundos que se queda el jugador viendo la zona restaurada antes de regresar.")]
    [SerializeField, Min(0f)] private float segundosAntesDeRegresar = 3f;

    [Tooltip("Cura al jugador al máximo al regresar al Hub.")]
    [SerializeField] private bool restablecerVidaAlRegresar = true;

    [Header("Eventos")]
    [Tooltip("Se invoca al llegar a la meta (sonido, partículas, música de victoria...).")]
    public UnityEvent AlLlegarMeta;

    private bool activada = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        if (mensajeMeta != null) mensajeMeta.SetActive(false);

        bool yaCompletado = ProgresoJuego.EstaCompletado(idNivelCompletado);
        AplicarRestauracion(yaCompletado && mostrarRestauradoSiYaCompletado);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activada) return;
        if (!other.CompareTag("Player")) return;

        StartCoroutine(RutinaMeta());
    }

    private IEnumerator RutinaMeta()
    {
        activada = true;

        ProgresoJuego.MarcarCompletado(idNivelCompletado);
        AplicarRestauracion(true);

        if (ocultarEnemigosAlLlegar) OcultarEnemigos();
        if (mensajeMeta != null) mensajeMeta.SetActive(true);

        AlLlegarMeta?.Invoke();
        DarRecompensa();

        // WaitForSeconds usa el tiempo escalado: si el panel de la habilidad
        // está abierto (Time.timeScale = 0), la espera empieza al cerrarlo.
        yield return new WaitForSeconds(segundosAntesDeRegresar);

        GestorNiveles.CargarNivel(nombreEscenaHub, idPuntoAparicionHub, restablecerVidaAlRegresar);
    }

    private void DarRecompensa()
    {
        if (indiceHabilidad < 0) return;

        JugadorPersistente jugador = JugadorPersistente.Obtener();
        if (jugador == null || jugador.Habilidades == null) return;

        // Si el jugador repite el nivel, no se vuelve a mostrar el panel.
        bool yaLaTenia = jugador.Habilidades.EstaDesbloqueada(indiceHabilidad);
        jugador.Habilidades.DesbloquearHabilidad(indiceHabilidad);

        Habilidad habilidad = jugador.Habilidades.ObtenerHabilidad(indiceHabilidad);
        if (!yaLaTenia && panelDesbloqueo != null && habilidad != null)
            panelDesbloqueo.Mostrar(habilidad);
    }

    private void AplicarRestauracion(bool restaurado)
    {
        if (objetosDeteriorados != null)
            foreach (GameObject objeto in objetosDeteriorados)
                if (objeto != null) objeto.SetActive(!restaurado);

        if (objetosRestaurados != null)
            foreach (GameObject objeto in objetosRestaurados)
                if (objeto != null) objeto.SetActive(restaurado);
    }

    private void OcultarEnemigos()
    {
        EnemigoIA[] enemigos = FindObjectsByType<EnemigoIA>(FindObjectsSortMode.None);
        foreach (EnemigoIA enemigo in enemigos)
            if (enemigo.gameObject.scene == gameObject.scene)
                enemigo.gameObject.SetActive(false);
    }
}
