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

    private const string LLAVE_MUSICA = "volumenMusica";
    private const string LLAVE_EFECTOS = "volumenEfectos";
    private const string LLAVE_PANTALLA_COMPLETA = "pantallaCompleta";

    public float VolumenMusicaActual { get; private set; } = 0.75f;
    public float VolumenEfectosActual { get; private set; } = 0.75f;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);

        CargarPreferencias();
    }

    private void CargarPreferencias()
    {
        VolumenMusicaActual = PlayerPrefs.GetFloat(LLAVE_MUSICA, 0.75f);
        VolumenEfectosActual = PlayerPrefs.GetFloat(LLAVE_EFECTOS, 0.75f);
        bool pantallaCompleta = PlayerPrefs.GetInt(LLAVE_PANTALLA_COMPLETA, 1) == 1;

        AplicarVolumenMusica(VolumenMusicaActual);
        AplicarVolumenEfectos(VolumenEfectosActual);
        Screen.fullScreen = pantallaCompleta;
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
