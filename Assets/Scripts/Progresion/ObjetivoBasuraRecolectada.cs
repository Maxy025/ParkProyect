using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Objetivo de nivel: recolectar cierta cantidad de basura (ej. 10 en la plaza, nivel 3).
/// Puede contar la basura al RECOGERLA del suelo o al DEPOSITARLA en un bote.
/// Como la habilidad del nivel 3 se da al inicio, normalmente se activa al cargar la escena.
/// Al completarse invoca AlCompletarObjetivo (engancha ahí MetaNivel.CompletarNivel()).
/// </summary>
public class ObjetivoBasuraRecolectada : MonoBehaviour
{
    public enum ModoConteo
    {
        [Tooltip("Cuenta cada basura en cuanto el jugador la recoge del suelo.")]
        AlRecoger,
        [Tooltip("Cuenta la basura solo cuando el jugador la tira en un bote.")]
        AlDepositar
    }

    [Header("Requisito")]
    [Tooltip("Cantidad de basura que hay que recolectar.")]
    [SerializeField, Min(1)] private int basurasRequeridas = 10;

    [Tooltip("AlRecoger = cuenta al levantarla. AlDepositar = cuenta al tirarla en el bote " +
             "(recuerda que el jugador solo carga 5 a la vez).")]
    [SerializeField] private ModoConteo modoConteo = ModoConteo.AlRecoger;

    [Tooltip("Basuras que cuentan (solo en modo AlRecoger). Si se deja vacío, cuentan todas las de esta escena.")]
    [SerializeField] private ObjetoBasura[] basurasObjetivo;

    [Tooltip("Empieza a contar al cargar la escena (nivel 3: la habilidad se tiene desde el inicio).")]
    [SerializeField] private bool activarAlIniciar = true;

    [Tooltip("Si este Id ya está completado (ej. Nivel3_Completado), el objetivo no se activa al repetir el nivel.")]
    [SerializeField] private string idNivelOmitirSiCompletado = ProgresoJuego.Nivel3;

    [Header("UI (opcional)")]
    [Tooltip("Texto que muestra el progreso. Se oculta hasta que el objetivo se activa.")]
    [SerializeField] private TMP_Text textoContador;

    [Tooltip("{0} = recolectadas, {1} = requeridas.")]
    [SerializeField] private string formatoTexto = "Basura recolectada: {0}/{1}";

    [SerializeField] private string textoCompletado = "¡La plaza ha sido restaurada!";

    [Header("Sonidos")]
    [Tooltip("Suena cada vez que el contador sube.")]
    [SerializeField] private SonidoAleatorio sonidoConteo = new SonidoAleatorio();
    [Tooltip("Suena al completar el objetivo.")]
    [SerializeField] private SonidoAleatorio sonidoCompletado = new SonidoAleatorio();

    [Header("Eventos")]
    [Tooltip("Se invoca cada vez que sube el conteo (envía el total recolectado).")]
    public EventoEntero AlCambiarConteo = new EventoEntero();

    [Tooltip("Se invoca una sola vez al llegar a la cantidad requerida. Engancha aquí MetaNivel.CompletarNivel().")]
    public UnityEvent AlCompletarObjetivo = new UnityEvent();

    private int recolectadas;
    private bool activo;
    private bool completado;

    public int Recolectadas => recolectadas;
    public int Requeridas => basurasRequeridas;
    public bool Activo => activo;
    public bool Completado => completado;

    private void Start()
    {
        if (textoContador != null) textoContador.gameObject.SetActive(false);

        if (activarAlIniciar) Activar();
    }

    private void OnDestroy()
    {
        BoteBasura.AlDepositarEnCualquierBote -= RegistrarDeposito;

        if (basurasObjetivo == null) return;

        foreach (ObjetoBasura basura in basurasObjetivo)
            if (basura != null && basura.AlRecolectar != null)
                basura.AlRecolectar.RemoveListener(RegistrarBasuraRecogida);
    }

    /// <summary>Empieza a contar la basura y muestra el contador.</summary>
    public void Activar()
    {
        if (activo || completado) return;
        if (ProgresoJuego.EstaCompletado(idNivelOmitirSiCompletado)) return;
        activo = true;

        if (modoConteo == ModoConteo.AlDepositar)
        {
            BoteBasura.AlDepositarEnCualquierBote += RegistrarDeposito;
        }
        else
        {
            if (basurasObjetivo == null || basurasObjetivo.Length == 0)
                BuscarBasurasDeLaEscena();

            int validas = 0;
            foreach (ObjetoBasura basura in basurasObjetivo)
            {
                if (basura == null) continue;
                if (basura.AlRecolectar == null) basura.AlRecolectar = new UnityEvent();
                basura.AlRecolectar.AddListener(RegistrarBasuraRecogida);
                validas++;
            }

            if (validas < basurasRequeridas)
                Debug.LogWarning($"[ObjetivoBasuraRecolectada] Se piden {basurasRequeridas} basuras, pero solo hay {validas}. El objetivo no se podrá completar.", this);
        }

        if (textoContador != null) textoContador.gameObject.SetActive(true);
        ActualizarTexto();
    }

    /// <summary>Suma una basura recogida. Lo llama el evento AlRecolectar de cada ObjetoBasura.</summary>
    public void RegistrarBasuraRecogida()
    {
        Sumar(1);
    }

    /// <summary>Suma la basura depositada en un bote (modo AlDepositar).</summary>
    private void RegistrarDeposito(int cantidad)
    {
        Sumar(cantidad);
    }

    private void Sumar(int cantidad)
    {
        if (!activo || completado || cantidad <= 0) return;

        recolectadas = Mathf.Min(recolectadas + cantidad, basurasRequeridas);
        AlCambiarConteo?.Invoke(recolectadas);
        ActualizarTexto();

        if (recolectadas >= basurasRequeridas)
        {
            completado = true;
            BoteBasura.AlDepositarEnCualquierBote -= RegistrarDeposito;
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
            : string.Format(formatoTexto, recolectadas, basurasRequeridas);
    }

    /// <summary>Si no se asignaron basuras, toma todas las activas de esta misma escena.</summary>
    private void BuscarBasurasDeLaEscena()
    {
        ObjetoBasura[] todas = FindObjectsByType<ObjetoBasura>(FindObjectsSortMode.None);
        basurasObjetivo = System.Array.FindAll(todas, b => b.gameObject.scene == gameObject.scene);
    }
}
