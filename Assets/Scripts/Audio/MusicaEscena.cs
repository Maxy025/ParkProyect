using UnityEngine;

/// <summary>
/// Define la música y el sonido ambiental de una escena. Colócalo en un objeto
/// vacío de cada escena (Menú, Hub, Nivel 1, 2, 3) y arrastra los clips.
/// Al cargar la escena le pide a GestorAudio que cambie de pista con un fundido.
/// Si la escena anterior usaba el mismo clip, la música continúa sin cortarse.
///
/// Si no hay GestorAudio (por ejemplo, al probar un nivel directamente sin pasar
/// por el menú), reproduce la música aquí mismo para poder probar igual.
/// </summary>
public class MusicaEscena : MonoBehaviour
{
    [Header("Música")]
    [Tooltip("Pista de música de esta escena (en bucle). Vacío = sin música.")]
    [SerializeField] private AudioClip musica;
    [SerializeField, Range(0f, 1f)] private float volumenMusica = 0.6f;

    [Header("Ambiente (opcional)")]
    [Tooltip("Sonido ambiental en bucle (pájaros, viento, ciudad...). Vacío = sin ambiente.")]
    [SerializeField] private AudioClip ambiente;
    [SerializeField, Range(0f, 1f)] private float volumenAmbiente = 0.4f;

    [Header("Transición")]
    [Tooltip("Segundos que dura el cambio entre la pista anterior y esta.")]
    [SerializeField, Min(0f)] private float duracionFundido = 1.5f;

    private void Start()
    {
        GestorAudio gestor = GestorAudio.Obtener();

        if (gestor != null)
        {
            gestor.ReproducirMusica(musica, volumenMusica, duracionFundido);
            gestor.ReproducirAmbiente(ambiente, volumenAmbiente, duracionFundido);
            return;
        }

        // Respaldo para pruebas sin GestorAudio: sin mixer ni transición.
        ReproducirLocal(musica, volumenMusica);
        ReproducirLocal(ambiente, volumenAmbiente);
    }

    private void ReproducirLocal(AudioClip clip, float volumen)
    {
        if (clip == null) return;

        AudioSource fuente = gameObject.AddComponent<AudioSource>();
        fuente.clip = clip;
        fuente.volume = volumen;
        fuente.loop = true;
        fuente.spatialBlend = 0f;
        fuente.Play();
    }
}
