using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manager persistente de audio (DontDestroyOnLoad). Controla el volumen de
/// música y efectos a través de un Audio Mixer, y guarda las preferencias del
/// jugador con PlayerPrefs para que se mantengan entre escenas y sesiones.
///
/// Colócalo en un GameObject en la escena del Menú de Inicio (la primera que
/// carga tu juego) — se mantiene vivo automáticamente en las escenas siguientes,
/// así que no hace falta repetirlo en el hub, niveles, etc.
///
/// También reproduce la música y el sonido ambiental de cada escena (ver MusicaEscena),
/// con transición suave entre pistas.
/// </summary>
public class GestorAudio : MonoBehaviour
{
    private static GestorAudio instancia;

    [Header("Audio Mixer")]
    [Tooltip("Debe tener 2 parámetros expuestos con estos nombres exactos " +
             "(clic derecho en el Volume de cada grupo > Expose to script).")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string parametroMusica = "VolumenMusica";
    [SerializeField] private string parametroEfectos = "VolumenEfectos";

    [Header("Grupos del mixer (para el audio creado por código)")]
    [Tooltip("Grupo 'Musica' del MixerPrincipal. Ahí sale la música de MusicaEscena.")]
    [SerializeField] private AudioMixerGroup grupoMusica;
    [Tooltip("Grupo 'Efectos' del MixerPrincipal. Ahí salen todos los efectos y el sonido ambiental.")]
    [SerializeField] private AudioMixerGroup grupoEfectos;

    private const string LLAVE_MUSICA = "volumenMusica";
    private const string LLAVE_EFECTOS = "volumenEfectos";
    private const string LLAVE_PANTALLA_COMPLETA = "pantallaCompleta";

    public float VolumenMusicaActual { get; private set; } = 0.75f;
    public float VolumenEfectosActual { get; private set; } = 0.75f;

    public AudioMixerGroup GrupoMusica => grupoMusica;
    public AudioMixerGroup GrupoEfectos => grupoEfectos;

    // En WebGL el AudioMixer no controla el volumen, así que ahí se aplica por código.
#if UNITY_WEBGL && !UNITY_EDITOR
    private static readonly bool volumenPorCodigo = true;
#else
    private static readonly bool volumenPorCodigo = false;
#endif

    /// <summary>Multiplicador de volumen de música aplicado por código (1 fuera de WebGL).</summary>
    public float FactorMusicaManual => volumenPorCodigo ? VolumenMusicaActual : 1f;

    /// <summary>Multiplicador de volumen de efectos aplicado por código (1 fuera de WebGL).</summary>
    public float FactorEfectosManual => volumenPorCodigo ? VolumenEfectosActual : 1f;

    /// <summary>Clip de música que está sonando (null si no hay).</summary>
    public AudioClip MusicaActual => canalMusica != null ? canalMusica.fuente.clip : null;

    /// <summary>Volumen de la música actual (el que se pidió en ReproducirMusica).</summary>
    public float VolumenMusicaPista => canalMusica != null ? canalMusica.volumenBase : 1f;

    // Un "canal" es un AudioSource en bucle con su volumen base y su fundido (0-1).
    private class Canal
    {
        public AudioSource fuente;
        public float volumenBase = 1f;
        public float fundido = 0f;
        public Coroutine rutina;
    }

    private Canal canalMusica;
    private Canal canalAmbiente;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);

        canalMusica = CrearCanal("Musica", grupoMusica);
        canalAmbiente = CrearCanal("Ambiente", grupoEfectos);

        CargarPreferencias();
    }

    private void Update()
    {
        // El volumen final se recalcula cada frame: volumen de la pista × fundido × ajuste (solo WebGL).
        ActualizarVolumen(canalMusica, FactorMusicaManual);
        ActualizarVolumen(canalAmbiente, FactorEfectosManual);
    }

    // ───────────────────────── Música y ambiente ─────────────────────────

    /// <summary>
    /// Cambia la música con un fundido. Si ya está sonando el mismo clip, sigue sin cortarse
    /// (así la música no se reinicia al ir y volver entre escenas que comparten pista).
    /// clip = null detiene la música.
    /// </summary>
    public void ReproducirMusica(AudioClip clip, float volumen = 1f, float duracionFundido = 1f)
    {
        CambiarPista(canalMusica, clip, volumen, duracionFundido);
    }

    /// <summary>Cambia el sonido ambiental en bucle (viento, pájaros, ciudad...). null lo detiene.</summary>
    public void ReproducirAmbiente(AudioClip clip, float volumen = 1f, float duracionFundido = 1f)
    {
        CambiarPista(canalAmbiente, clip, volumen, duracionFundido);
    }

    /// <summary>Detiene la música con un fundido.</summary>
    public void DetenerMusica(float duracionFundido = 1f)
    {
        CambiarPista(canalMusica, null, 0f, duracionFundido);
    }

    /// <summary>Detiene el sonido ambiental con un fundido.</summary>
    public void DetenerAmbiente(float duracionFundido = 1f)
    {
        CambiarPista(canalAmbiente, null, 0f, duracionFundido);
    }

    private Canal CrearCanal(string nombre, AudioMixerGroup grupo)
    {
        GameObject hijo = new GameObject(nombre);
        hijo.transform.SetParent(transform);

        AudioSource fuente = hijo.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
        fuente.loop = true;
        fuente.spatialBlend = 0f;
        fuente.ignoreListenerPause = true;
        fuente.outputAudioMixerGroup = grupo;
        fuente.volume = 0f;

        return new Canal { fuente = fuente };
    }

    private void CambiarPista(Canal canal, AudioClip clip, float volumen, float duracionFundido)
    {
        if (canal == null) return;

        // Mismo clip sonando: solo se ajusta el volumen.
        if (clip != null && canal.fuente.clip == clip && canal.fuente.isPlaying)
        {
            canal.volumenBase = volumen;
            return;
        }

        if (canal.rutina != null) StopCoroutine(canal.rutina);
        canal.rutina = StartCoroutine(RutinaCambiarPista(canal, clip, volumen, Mathf.Max(0f, duracionFundido)));
    }

    private IEnumerator RutinaCambiarPista(Canal canal, AudioClip clip, float volumen, float duracion)
    {
        // Se usa tiempo sin escalar: la música debe cambiar aunque el juego esté en pausa.
        float mitad = duracion * 0.5f;

        // 1) Baja la pista anterior.
        if (canal.fuente.isPlaying)
        {
            while (canal.fundido > 0f)
            {
                canal.fundido = mitad > 0f ? canal.fundido - Time.unscaledDeltaTime / mitad : 0f;
                yield return null;
            }
            canal.fuente.Stop();
        }

        canal.fundido = 0f;
        canal.fuente.clip = clip;
        canal.volumenBase = volumen;

        if (clip == null)
        {
            canal.rutina = null;
            yield break;
        }

        // 2) Sube la pista nueva.
        canal.fuente.Play();
        while (canal.fundido < 1f)
        {
            canal.fundido = mitad > 0f ? canal.fundido + Time.unscaledDeltaTime / mitad : 1f;
            yield return null;
        }

        canal.fundido = 1f;
        canal.rutina = null;
    }

    private void ActualizarVolumen(Canal canal, float factor)
    {
        if (canal == null) return;
        canal.fuente.volume = canal.volumenBase * Mathf.Clamp01(canal.fundido) * factor;
    }

    private void CargarPreferencias()
    {
        VolumenMusicaActual = PlayerPrefs.GetFloat(LLAVE_MUSICA, 0.75f);
        VolumenEfectosActual = PlayerPrefs.GetFloat(LLAVE_EFECTOS, 0.75f);
        bool pantallaCompleta = PlayerPrefs.GetInt(LLAVE_PANTALLA_COMPLETA, 1) == 1;

        AplicarVolumenMusica(VolumenMusicaActual);
        AplicarVolumenEfectos(VolumenEfectosActual);

        // En WebGL el navegador solo permite entrar a pantalla completa tras un clic del jugador.
        // Si se pide al cargar, Unity lo aplica en el PRIMER clic (por ejemplo en "Jugar"),
        // lo cual sorprende al jugador. En web se deja en ventana y se activa desde Ajustes.
#if !UNITY_WEBGL || UNITY_EDITOR
        Screen.fullScreen = pantallaCompleta;
#endif
    }

    public void EstablecerVolumenMusica(float volumen01)
    {
        VolumenMusicaActual = volumen01;
        AplicarVolumenMusica(volumen01);
        PlayerPrefs.SetFloat(LLAVE_MUSICA, volumen01);
    }

    public void EstablecerVolumenEfectos(float volumen01)
    {
        VolumenEfectosActual = volumen01;
        AplicarVolumenEfectos(volumen01);
        PlayerPrefs.SetFloat(LLAVE_EFECTOS, volumen01);
    }

    public void EstablecerPantallaCompleta(bool activa)
    {
        Screen.fullScreen = activa;
        PlayerPrefs.SetInt(LLAVE_PANTALLA_COMPLETA, activa ? 1 : 0);
    }

    private void AplicarVolumenMusica(float volumen01)
    {
        if (mixer != null) mixer.SetFloat(parametroMusica, ConvertirADecibeles(volumen01));
    }

    private void AplicarVolumenEfectos(float volumen01)
    {
        if (mixer != null) mixer.SetFloat(parametroEfectos, ConvertirADecibeles(volumen01));
    }

    // El mixer trabaja en decibeles (escala logarítmica) y los sliders son más
    // intuitivos en 0-1 (escala lineal); esta conversión es la estándar de Unity.
    private float ConvertirADecibeles(float volumen01)
    {
        return Mathf.Log10(Mathf.Clamp(volumen01, 0.0001f, 1f)) * 20f;
    }

    public static GestorAudio Obtener() => instancia;
}
