using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Objetivo de escena: al vencer cierta cantidad de enemigos se desbloquean objetos
/// (por ejemplo, el portal al nivel 1 del Hub).
/// El progreso se guarda en ProgresoJuego para que no se pierda al cambiar de escena.
/// </summary>
public class ObjetivoEnemigosVencidos : MonoBehaviour
{
    [Header("Identificación")]
    [Tooltip("Id único del objetivo. Sirve para recordar que ya se completó al volver a la escena.")]
    [SerializeField] private string idObjetivo = "Hub_Portal1";

    [Header("Requisito")]
    [Tooltip("Cantidad de enemigos que hay que vencer.")]
    [SerializeField, Min(1)] private int enemigosRequeridos = 5;

    [Tooltip("Enemigos que cuentan para este objetivo. Si se deja vacío, cuentan todos los enemigos de esta escena.")]
    [SerializeField] private VidaEnemigo[] enemigosObjetivo;

    [Header("Al completar")]
    [Tooltip("Objetos que se ACTIVAN al completar (ej. el portal). Déjalos activos en la escena: el script los oculta al iniciar.")]
    [SerializeField] private GameObject[] objetosADesbloquear;

    [Tooltip("Objetos que se DESACTIVAN al completar (ej. una barrera o un portal apagado).")]
    [SerializeField] private GameObject[] objetosABloquear;

    [Tooltip("Si el objetivo ya estaba completado al cargar la escena, oculta a los enemigos para que el Hub no se vuelva a llenar.")]
    [SerializeField] private bool ocultarEnemigosSiYaCompletado = true;

    [Header("UI (opcional)")]
    [Tooltip("Texto que muestra el progreso.")]
    [SerializeField] private TMP_Text textoContador;

    [Tooltip("{0} = vencidos, {1} = requeridos.")]
    [SerializeField] private string formatoTexto = "Enemigos vencidos: {0}/{1}";

    [SerializeField] private string textoCompletado = "¡El portal se ha abierto!";

    [Tooltip("Si se activa, el texto se oculta cuando el objetivo ya estaba completado al cargar la escena.")]
    [SerializeField] private bool ocultarTextoSiYaCompletado = true;

    [Header("Eventos")]
    [Tooltip("Se invoca cada vez que se vence un enemigo (envía el total de vencidos).")]
    public EventoEntero AlCambiarConteo = new EventoEntero();

    [Tooltip("Se invoca una sola vez en el momento en que se completa el objetivo (sonido, partículas, mensaje...).")]
    public UnityEvent AlCompletarObjetivo = new UnityEvent();

    private int vencidos;
    private bool completado;

    public int Vencidos => vencidos;
    public int Requeridos => enemigosRequeridos;
    public bool Completado => completado;

    private void Start()
    {
        if (enemigosObjetivo == null || enemigosObjetivo.Length == 0)
            BuscarEnemigosDeLaEscena();

        // Si ya se completó en otra visita a la escena, se deja todo desbloqueado.
        if (ProgresoJuego.EstaCompletado(idObjetivo))
        {
            completado = true;
            vencidos = enemigosRequeridos;

            if (ocultarEnemigosSiYaCompletado)
                foreach (VidaEnemigo enemigo in enemigosObjetivo)
                    if (enemigo != null) enemigo.gameObject.SetActive(false);

            AplicarDesbloqueo(true);

            if (textoContador != null && ocultarTextoSiYaCompletado)
                textoContador.gameObject.SetActive(false);
            else
                ActualizarTexto();
            return;
        }

        // Estado inicial: bloqueado.
        AplicarDesbloqueo(false);

        int validos = 0;
        foreach (VidaEnemigo enemigo in enemigosObjetivo)
        {
            if (enemigo == null) continue;
            enemigo.AlMorir.AddListener(RegistrarEnemigoVencido);
            validos++;
        }

        if (validos < enemigosRequeridos)
            Debug.LogWarning($"[ObjetivoEnemigosVencidos] '{idObjetivo}' pide {enemigosRequeridos} enemigos, pero solo hay {validos} asignados. El objetivo no se podrá completar.", this);

        ActualizarTexto();
    }

    private void OnDestroy()
    {
        if (enemigosObjetivo == null) return;

        foreach (VidaEnemigo enemigo in enemigosObjetivo)
            if (enemigo != null) enemigo.AlMorir.RemoveListener(RegistrarEnemigoVencido);
    }

    /// <summary>Suma un enemigo vencido. Lo llama el evento AlMorir de cada VidaEnemigo.</summary>
    public void RegistrarEnemigoVencido()
    {
        if (completado) return;

        vencidos++;
        AlCambiarConteo?.Invoke(vencidos);
        ActualizarTexto();

        if (vencidos >= enemigosRequeridos)
            Completar();
    }

    private void Completar()
    {
        completado = true;
        ProgresoJuego.MarcarCompletado(idObjetivo);
        AplicarDesbloqueo(true);
        ActualizarTexto();
        AlCompletarObjetivo?.Invoke();
    }

    private void AplicarDesbloqueo(bool desbloqueado)
    {
        if (objetosADesbloquear != null)
            foreach (GameObject objeto in objetosADesbloquear)
                if (objeto != null) objeto.SetActive(desbloqueado);

        if (objetosABloquear != null)
            foreach (GameObject objeto in objetosABloquear)
                if (objeto != null) objeto.SetActive(!desbloqueado);
    }

    private void ActualizarTexto()
    {
        if (textoContador == null) return;

        textoContador.text = completado
            ? textoCompletado
            : string.Format(formatoTexto, vencidos, enemigosRequeridos);
    }

    /// <summary>Si no se asignaron enemigos, toma todos los de esta misma escena.</summary>
    private void BuscarEnemigosDeLaEscena()
    {
        VidaEnemigo[] todos = FindObjectsByType<VidaEnemigo>(FindObjectsSortMode.None);
        enemigosObjetivo = System.Array.FindAll(todos, e => e.gameObject.scene == gameObject.scene);
    }
}
