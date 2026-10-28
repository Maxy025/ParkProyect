using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Objetivo de nivel: cortar cierta cantidad de maleza (ej. 10 en el camellón).
/// Empieza "dormido" (sin contar y con el texto oculto) hasta que se llama a Activar(),
/// normalmente desde MetaNivel.AlIniciarObjetivoExtra, cuando el jugador ya tiene las tijeras.
/// Al completarse invoca AlCompletarObjetivo (engancha ahí MetaNivel.CompletarNivel()).
/// </summary>
public class ObjetivoMalezasCortadas : MonoBehaviour
{
    [Header("Requisito")]
    [Tooltip("Cantidad de maleza que hay que cortar.")]
    [SerializeField, Min(1)] private int malezasRequeridas = 10;

    [Tooltip("Malezas que cuentan para este objetivo. Si se deja vacío, cuentan todas las de esta escena.")]
    [SerializeField] private Maleza[] malezasObjetivo;

    [Tooltip("Empieza a contar al cargar la escena sin esperar a que lo active otro script.")]
    [SerializeField] private bool activarAlIniciar = false;

    [Header("UI (opcional)")]
    [Tooltip("Texto que muestra el progreso. Se oculta hasta que el objetivo se activa.")]
    [SerializeField] private TMP_Text textoContador;

    [Tooltip("{0} = cortadas, {1} = requeridas.")]
    [SerializeField] private string formatoTexto = "Malezas cortadas: {0}/{1}";

    [SerializeField] private string textoCompletado = "¡El camellón ha sido restaurado!";

    [Header("Eventos")]
    [Tooltip("Se invoca cada vez que se corta una maleza (envía el total de cortadas).")]
    public EventoEntero AlCambiarConteo = new EventoEntero();

    [Tooltip("Se invoca una sola vez al cortar la cantidad requerida. Engancha aquí MetaNivel.CompletarNivel().")]
    public UnityEvent AlCompletarObjetivo = new UnityEvent();

    private int cortadas;
    private bool activo;
    private bool completado;

    public int Cortadas => cortadas;
    public int Requeridas => malezasRequeridas;
    public bool Activo => activo;
    public bool Completado => completado;

    private void Start()
    {
        if (textoContador != null) textoContador.gameObject.SetActive(false);

        if (activarAlIniciar) Activar();
    }

    private void OnDestroy()
    {
        if (malezasObjetivo == null) return;

        foreach (Maleza maleza in malezasObjetivo)
            if (maleza != null && maleza.AlCortar != null)
                maleza.AlCortar.RemoveListener(RegistrarMalezaCortada);
    }

    /// <summary>Empieza a contar la maleza cortada y muestra el contador.</summary>
    public void Activar()
    {
        if (activo || completado) return;
        activo = true;

        if (malezasObjetivo == null || malezasObjetivo.Length == 0)
            BuscarMalezasDeLaEscena();

        int validas = 0;
        foreach (Maleza maleza in malezasObjetivo)
        {
            if (maleza == null) continue;
            if (maleza.AlCortar == null) maleza.AlCortar = new UnityEvent();
            maleza.AlCortar.AddListener(RegistrarMalezaCortada);
            validas++;
        }

        if (validas < malezasRequeridas)
            Debug.LogWarning($"[ObjetivoMalezasCortadas] Se piden {malezasRequeridas} malezas, pero solo hay {validas}. El objetivo no se podrá completar.", this);

        if (textoContador != null) textoContador.gameObject.SetActive(true);
        ActualizarTexto();
    }

    /// <summary>Suma una maleza cortada. Lo llama el evento AlCortar de cada Maleza.</summary>
    public void RegistrarMalezaCortada()
    {
        if (!activo || completado) return;

        cortadas++;
        AlCambiarConteo?.Invoke(cortadas);
        ActualizarTexto();

        if (cortadas >= malezasRequeridas)
        {
            completado = true;
            ActualizarTexto();
            AlCompletarObjetivo?.Invoke();
        }
    }

    private void ActualizarTexto()
    {
        if (textoContador == null) return;

        textoContador.text = completado
            ? textoCompletado
            : string.Format(formatoTexto, cortadas, malezasRequeridas);
    }

    /// <summary>Si no se asignaron malezas, toma todas las activas de esta misma escena.</summary>
    private void BuscarMalezasDeLaEscena()
    {
        Maleza[] todas = FindObjectsByType<Maleza>(FindObjectsSortMode.None);
        malezasObjetivo = System.Array.FindAll(todas, m => m.gameObject.scene == gameObject.scene);
    }
}
