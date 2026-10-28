using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Línea de meta de un nivel. Funciona de dos formas:
///
/// A) Sin objetivo extra: al tocar la meta se da la habilidad y se completa el nivel
///    (restauración visual, cuenta regresiva y regreso al Hub).
///
/// B) Con objetivo extra (ej. nivel 1): al tocar la meta solo se da la habilidad y se
///    invoca AlIniciarObjetivoExtra (ej. ObjetivoMalezasCortadas.Activar). El nivel se
///    completa cuando ese objetivo llama a CompletarNivel().
///
/// Al completar: se marca el Id en ProgresoJuego, se apagan los objetos "deteriorados",
/// se encienden los "restaurados", se muestra la cuenta regresiva y se regresa al Hub.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MetaNivel : MonoBehaviour
{
    [Header("Nivel")]
    [Tooltip("Id que se guarda en ProgresoJuego al completar el nivel. Usa Nivel1_Completado, Nivel2_Completado o Nivel3_Completado.")]
    [SerializeField] private string idNivelCompletado = ProgresoJuego.Nivel1;

    [Header("Objetivo extra (opcional)")]
    [Tooltip("Si está marcado, al llegar a la meta NO se completa el nivel: se invoca AlIniciarObjetivoExtra " +
             "y se espera a que algo llame a CompletarNivel() (ej. AlCompletarObjetivo de ObjetivoMalezasCortadas).")]
    [SerializeField] private bool esperarObjetivoExtra = false;

    [Tooltip("Objetos que se ocultan al tocar la meta (ej. el modelo de la recompensa).")]
    [SerializeField] private GameObject[] objetosOcultarAlLlegar;

    [Header("Restauración visual")]
    [Tooltip("Objetos del nivel descuidado (basura, pasto seco, maleza...). Se DESACTIVAN al completar el nivel.")]
    [SerializeField] private GameObject[] objetosDeteriorados;

    [Tooltip("Objetos del nivel restaurado (flores, árboles sanos, bancas nuevas...). Se ACTIVAN al completar el nivel. Pueden estar desactivados en la escena.")]
    [SerializeField] private GameObject[] objetosRestaurados;

    [Tooltip("Si el nivel ya se había completado antes, al volver a entrar se muestra restaurado desde el inicio.")]
    [SerializeField] private bool mostrarRestauradoSiYaCompletado = true;

    [Tooltip("Oculta a los enemigos que sigan vivos al completar el nivel, para que no ataquen durante la cuenta regresiva.")]
    [SerializeField] private bool ocultarEnemigosAlLlegar = true;

    [Tooltip("Si el nivel ya se había completado antes, al volver a entrar se ocultan los enemigos de la escena.")]
    [SerializeField] private bool ocultarEnemigosSiYaCompletado = true;

    [Tooltip("Si el nivel ya se había completado antes, al volver a entrar se ocultan las malezas de la escena.")]
    [SerializeField] private bool ocultarMalezasSiYaCompletado = true;

    [Header("Recompensa (opcional)")]
    [Tooltip("-1 = ninguna (nivel 3) · 0 = Tijeras (nivel 1) · 1 = Regar (nivel 2) · 2 = Basura")]
    [SerializeField] private int indiceHabilidad = -1;

    [Tooltip("Panel que muestra la habilidad desbloqueada (congela el juego mientras está abierto).")]
    [SerializeField] private PanelHabilidadDesbloqueada panelDesbloqueo;

    [Header("Mensaje (opcional)")]
    [Tooltip("Objeto de UI que se enciende al COMPLETAR el nivel (ej. '¡Camellón restaurado!'). Se oculta al iniciar.")]
    [SerializeField] private GameObject mensajeMeta;

    [Header("Cuenta regresiva (opcional)")]
    [Tooltip("Texto que muestra los segundos que faltan para regresar al Hub. Se oculta al iniciar.")]
    [SerializeField] private TMP_Text textoCuentaRegresiva;

    [Tooltip("{0} = segundos restantes.")]
    [SerializeField] private string formatoCuentaRegresiva = "Regresando al parque en {0}...";

    [Header("Regreso al Hub")]
    [Tooltip("Nombre EXACTO de la escena del Hub (debe estar en Build Settings).")]
    [SerializeField] private string nombreEscenaHub = "Hub";

    [Tooltip("Id del PuntoAparicion del Hub donde aparece el jugador al regresar.")]
    [SerializeField] private string idPuntoAparicionHub = "DesdeNivel1";

    [Tooltip("Segundos de la cuenta regresiva antes de regresar al Hub.")]
    [SerializeField, Min(0f)] private float segundosAntesDeRegresar = 5f;

    [Tooltip("Cura al jugador al máximo al regresar al Hub.")]
    [SerializeField] private bool restablecerVidaAlRegresar = true;

    [Header("Eventos")]
    [Tooltip("Se invoca al tocar la meta (sonido, partículas...).")]
    public UnityEvent AlLlegarMeta;

    [Tooltip("Se invoca al tocar la meta SOLO si se espera un objetivo extra. Engancha aquí ObjetivoMalezasCortadas.Activar().")]
    public UnityEvent AlIniciarObjetivoExtra;

    [Tooltip("Se invoca al completar el nivel, justo antes de la cuenta regresiva (música de victoria...).")]
    public UnityEvent AlCompletarNivel;

    private bool llegada = false;
    private bool completado = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        if (mensajeMeta != null) mensajeMeta.SetActive(false);
        if (textoCuentaRegresiva != null) textoCuentaRegresiva.gameObject.SetActive(false);

        bool yaCompletado = ProgresoJuego.EstaCompletado(idNivelCompletado);
        AplicarRestauracion(yaCompletado && mostrarRestauradoSiYaCompletado);

        if (yaCompletado)
        {
            if (ocultarEnemigosSiYaCompletado) OcultarEnemigos();
            if (ocultarMalezasSiYaCompletado) OcultarMalezas();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (llegada) return;
        if (!other.CompareTag("Player")) return;

        llegada = true;

        if (objetosOcultarAlLlegar != null)
            foreach (GameObject objeto in objetosOcultarAlLlegar)
                if (objeto != null) objeto.SetActive(false);

        AlLlegarMeta?.Invoke();
        DarRecompensa();

        // Si el nivel ya se había completado antes (el jugador lo repite),
        // no se vuelve a pedir el objetivo extra.
        bool yaCompletado = ProgresoJuego.EstaCompletado(idNivelCompletado);

        if (esperarObjetivoExtra && !yaCompletado)
            AlIniciarObjetivoExtra?.Invoke();
        else
            CompletarNivel();
    }

    /// <summary>
    /// Completa el nivel: guarda el progreso, restaura la zona y regresa al Hub tras la cuenta regresiva.
    /// Engánchalo a AlCompletarObjetivo del objetivo extra.
    /// </summary>
    public void CompletarNivel()
    {
        if (completado) return;
        completado = true;

        StartCoroutine(RutinaCompletar());
    }

    private IEnumerator RutinaCompletar()
    {
        ProgresoJuego.MarcarCompletado(idNivelCompletado);
        AplicarRestauracion(true);

        if (ocultarEnemigosAlLlegar) OcultarEnemigos();
        if (mensajeMeta != null) mensajeMeta.SetActive(true);

        AlCompletarNivel?.Invoke();

        if (textoCuentaRegresiva != null) textoCuentaRegresiva.gameObject.SetActive(true);

        // Se usa Time.deltaTime: si hay un panel abierto (Time.timeScale = 0),
        // la cuenta se detiene hasta que el jugador lo cierre.
        float restante = segundosAntesDeRegresar;
        while (restante > 0f)
        {
            if (textoCuentaRegresiva != null)
                textoCuentaRegresiva.text = string.Format(formatoCuentaRegresiva, Mathf.CeilToInt(restante));

            yield return null;
            restante -= Time.deltaTime;
        }

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

    private void OcultarMalezas()
    {
        Maleza[] malezas = FindObjectsByType<Maleza>(FindObjectsSortMode.None);
        foreach (Maleza maleza in malezas)
            if (maleza.gameObject.scene == gameObject.scene)
                maleza.gameObject.SetActive(false);
    }
}
