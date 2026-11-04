using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Objetivo de nivel: limpiar cierta cantidad de zonas peligrosas (charcos) con la habilidad de regar
/// (ej. 10 en la zona residencial, nivel 2).
/// Empieza "dormido" (sin contar y con el texto oculto) hasta que se llama a Activar(),
/// normalmente desde MetaNivel.AlIniciarObjetivoExtra, cuando el jugador ya tiene la regadera.
/// Al completarse invoca AlCompletarObjetivo (engancha ahí MetaNivel.CompletarNivel()).
/// </summary>
public class ObjetivoCharcosLimpiados : MonoBehaviour
{
    [Header("Requisito")]
    [Tooltip("Cantidad de charcos que hay que limpiar.")]
    [SerializeField, Min(1)] private int charcosRequeridos = 10;

    [Tooltip("Charcos (ZonaPeligrosa) que cuentan para este objetivo. Si se deja vacío, cuentan todos los de esta escena.")]
    [SerializeField] private ZonaPeligrosa[] charcosObjetivo;

    [Tooltip("Empieza a contar al cargar la escena sin esperar a que lo active otro script.")]
    [SerializeField] private bool activarAlIniciar = false;

    [Tooltip("Si este Id ya está completado (ej. Nivel2_Completado), el objetivo no se activa al repetir el nivel.")]
    [SerializeField] private string idNivelOmitirSiCompletado = ProgresoJuego.Nivel2;

    [Header("UI (opcional)")]
    [Tooltip("Texto que muestra el progreso. Se oculta hasta que el objetivo se activa.")]
    [SerializeField] private TMP_Text textoContador;

    [Tooltip("{0} = limpiados, {1} = requeridos.")]
    [SerializeField] private string formatoTexto = "Charcos limpiados: {0}/{1}";

    [SerializeField] private string textoCompletado = "¡La zona residencial ha sido restaurada!";

    [Header("Sonidos")]
    [Tooltip("Suena cada vez que el contador sube.")]
    [SerializeField] private SonidoAleatorio sonidoConteo = new SonidoAleatorio();
    [Tooltip("Suena al completar el objetivo.")]
    [SerializeField] private SonidoAleatorio sonidoCompletado = new SonidoAleatorio();

    [Header("Eventos")]
    [Tooltip("Se invoca cada vez que se limpia un charco (envía el total de limpiados).")]
    public EventoEntero AlCambiarConteo = new EventoEntero();

    [Tooltip("Se invoca una sola vez al limpiar la cantidad requerida. Engancha aquí MetaNivel.CompletarNivel().")]
    public UnityEvent AlCompletarObjetivo = new UnityEvent();

    private int limpiados;
    private bool activo;
    private bool completado;

    public int Limpiados => limpiados;
    public int Requeridos => charcosRequeridos;
    public bool Activo => activo;
    public bool Completado => completado;

    private void Start()
    {
        if (textoContador != null) textoContador.gameObject.SetActive(false);

        if (activarAlIniciar) Activar();
    }

    private void OnDestroy()
    {
        if (charcosObjetivo == null) return;

        foreach (ZonaPeligrosa charco in charcosObjetivo)
            if (charco != null && charco.AlLimpiar != null)
                charco.AlLimpiar.RemoveListener(RegistrarCharcoLimpiado);
    }

    /// <summary>Empieza a contar los charcos limpiados y muestra el contador.</summary>
    public void Activar()
    {
        if (activo || completado) return;
        if (ProgresoJuego.EstaCompletado(idNivelOmitirSiCompletado)) return;
        activo = true;

        if (charcosObjetivo == null || charcosObjetivo.Length == 0)
            BuscarCharcosDeLaEscena();

        int validos = 0;
        foreach (ZonaPeligrosa charco in charcosObjetivo)
        {
            if (charco == null) continue;
            if (charco.AlLimpiar == null) charco.AlLimpiar = new UnityEvent();
            charco.AlLimpiar.AddListener(RegistrarCharcoLimpiado);
            validos++;
        }

        if (validos < charcosRequeridos)
            Debug.LogWarning($"[ObjetivoCharcosLimpiados] Se piden {charcosRequeridos} charcos, pero solo hay {validos}. El objetivo no se podrá completar.", this);

        if (textoContador != null) textoContador.gameObject.SetActive(true);
        ActualizarTexto();
    }

    /// <summary>Suma un charco limpiado. Lo llama el evento AlLimpiar de cada ZonaPeligrosa.</summary>
    public void RegistrarCharcoLimpiado()
    {
        if (!activo || completado) return;

        limpiados++;
        AlCambiarConteo?.Invoke(limpiados);
        ActualizarTexto();

        if (limpiados >= charcosRequeridos)
        {
            completado = true;
            ActualizarTexto();
            ReproductorAudio.Reproducir2D(sonidoCompletado);
            AlCompletarObjetivo?.Invoke();
        }
        else
        {
            ReproductorAudio.Reproducir2D(sonidoConteo);
        }
    }

    private void ActualizarTexto()
    {
        if (textoContador == null) return;

        textoContador.text = completado
            ? textoCompletado
            : string.Format(formatoTexto, limpiados, charcosRequeridos);
    }

    /// <summary>Si no se asignaron charcos, toma todas las ZonaPeligrosa activas de esta misma escena.</summary>
    private void BuscarCharcosDeLaEscena()
    {
        ZonaPeligrosa[] todas = FindObjectsByType<ZonaPeligrosa>(FindObjectsSortMode.None);
        charcosObjetivo = System.Array.FindAll(todas, z => z.gameObject.scene == gameObject.scene);
    }
}
