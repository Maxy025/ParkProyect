using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Punto único para reproducir efectos de sonido desde cualquier script.
/// - Usa un grupo de AudioSources reutilizables (pool) que viven entre escenas,
///   así un sonido sigue sonando aunque el objeto que lo pidió se destruya
///   (enemigo que muere, maleza cortada, cambio de escena por un portal).
/// - Envía todo al grupo "Efectos" del AudioMixer de GestorAudio, para que
///   respete el volumen de efectos del menú de ajustes.
/// - No reserva memoria en cada sonido (importante en WebGL).
/// </summary>
public static class ReproductorAudio
{
    // Cantidad de sonidos simultáneos. Si se llenan, se reutiliza el más antiguo.
    private const int CANTIDAD_FUENTES_2D = 6;
    private const int CANTIDAD_FUENTES_3D = 16;

    // Los sonidos 3D se ignoran durante este tiempo tras cargar una escena. Evita que
    // suenen al restaurar el estado (EstadoPersistente vuelve a "regar" y "limpiar" al cargar).
    private const float MARGEN_INICIO_ESCENA = 0.3f;

    private static AudioSource[] fuentes2D;
    private static AudioSource[] fuentes3D;
    private static int siguiente2D;
    private static int siguiente3D;

    /// <summary>Distancia a la que un sonido 3D deja de escucharse.</summary>
    public static float DistanciaMaxima3D = 30f;

    /// <summary>Grupo "Efectos" del mixer (null si no hay GestorAudio en la partida).</summary>
    public static AudioMixerGroup GrupoEfectos
    {
        get
        {
            GestorAudio gestor = GestorAudio.Obtener();
            return gestor != null ? gestor.GrupoEfectos : null;
        }
    }

    /// <summary>
    /// Multiplicador de volumen de efectos aplicado por código. Vale 1 salvo en WebGL,
    /// donde el AudioMixer no controla el volumen y hay que aplicarlo a mano.
    /// </summary>
    public static float FactorEfectos
    {
        get
        {
            GestorAudio gestor = GestorAudio.Obtener();
            return gestor != null ? gestor.FactorEfectosManual : 1f;
        }
    }

    /// <summary>
    /// Reproduce un sonido "plano" (sin posición): interfaz, menús, jingles.
    /// Suena aunque el juego esté en pausa (Time.timeScale = 0).
    /// </summary>
    public static void Reproducir2D(SonidoAleatorio sonido)
    {
        if (sonido == null) return;
        AudioClip clip = sonido.Elegir();
        if (clip == null) return;

        AsegurarFuentes();
        AudioSource fuente = TomarFuente(fuentes2D, ref siguiente2D);
        Configurar(fuente, clip, sonido.volumen, sonido.ElegirTono());
        fuente.Play();
    }

    /// <summary>
    /// Reproduce un sonido en un punto del mundo (se escucha más bajo al alejarse).
    /// mezclaEspacial: 1 = totalmente 3D, 0 = plano.
    /// </summary>
    public static void ReproducirEn(SonidoAleatorio sonido, Vector3 posicion, float mezclaEspacial = 1f)
    {
        if (sonido == null) return;
        if (Time.timeSinceLevelLoad < MARGEN_INICIO_ESCENA) return;

        AudioClip clip = sonido.Elegir();
        if (clip == null) return;

        AsegurarFuentes();
        AudioSource fuente = TomarFuente(fuentes3D, ref siguiente3D);
        fuente.transform.position = posicion;
        fuente.spatialBlend = Mathf.Clamp01(mezclaEspacial);
        fuente.maxDistance = DistanciaMaxima3D;
        Configurar(fuente, clip, sonido.volumen, sonido.ElegirTono());
        fuente.Play();
    }

    /// <summary>
    /// Agrega (o reutiliza) un AudioSource en el objeto indicado, ya conectado al grupo
    /// Efectos. Úsalo para sonidos en bucle que dependen de un objeto (ej. carga del agua).
    /// </summary>
    public static AudioSource CrearFuenteBucle(GameObject dueno, float mezclaEspacial = 1f)
    {
        AudioSource fuente = dueno.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
        fuente.loop = true;
        fuente.spatialBlend = Mathf.Clamp01(mezclaEspacial);
        fuente.rolloffMode = AudioRolloffMode.Linear;
        fuente.maxDistance = DistanciaMaxima3D;
        fuente.outputAudioMixerGroup = GrupoEfectos;
        return fuente;
    }

    private static void Configurar(AudioSource fuente, AudioClip clip, float volumen, float tono)
    {
        fuente.Stop();
        fuente.clip = clip;
        fuente.volume = volumen * FactorEfectos;
        fuente.pitch = tono;
        // Se asigna en cada sonido por si GestorAudio apareció después (ej. al venir del menú).
        fuente.outputAudioMixerGroup = GrupoEfectos;
    }

    private static AudioSource TomarFuente(AudioSource[] fuentes, ref int siguiente)
    {
        // Primero busca una libre; si todas suenan, reutiliza la más antigua (ronda).
        for (int i = 0; i < fuentes.Length; i++)
        {
            int indice = (siguiente + i) % fuentes.Length;
            if (!fuentes[indice].isPlaying)
            {
                siguiente = (indice + 1) % fuentes.Length;
                return fuentes[indice];
            }
        }

        AudioSource fuente = fuentes[siguiente];
        siguiente = (siguiente + 1) % fuentes.Length;
        return fuente;
    }

    private static void AsegurarFuentes()
    {
        if (fuentes2D != null && fuentes2D[0] != null) return;

        GameObject contenedor = new GameObject("[ReproductorAudio]");
        Object.DontDestroyOnLoad(contenedor);

        fuentes2D = new AudioSource[CANTIDAD_FUENTES_2D];
        for (int i = 0; i < fuentes2D.Length; i++)
        {
            fuentes2D[i] = contenedor.AddComponent<AudioSource>();
            fuentes2D[i].playOnAwake = false;
            fuentes2D[i].spatialBlend = 0f;
            fuentes2D[i].ignoreListenerPause = true;
        }

        fuentes3D = new AudioSource[CANTIDAD_FUENTES_3D];
        for (int i = 0; i < fuentes3D.Length; i++)
        {
            GameObject hijo = new GameObject("Fuente3D_" + i);
            hijo.transform.SetParent(contenedor.transform);
            fuentes3D[i] = hijo.AddComponent<AudioSource>();
            fuentes3D[i].playOnAwake = false;
            fuentes3D[i].rolloffMode = AudioRolloffMode.Linear;
            fuentes3D[i].minDistance = 2f;
        }

        siguiente2D = 0;
        siguiente3D = 0;
    }

    // Limpia las referencias al entrar a Play en el editor (por si la recarga de dominio está desactivada).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarAlIniciarJuego()
    {
        fuentes2D = null;
        fuentes3D = null;
    }
}
