using UnityEngine;

/// <summary>
/// Acompaña a un AudioSource en bucle colocado a mano en un prefab u objeto de la escena
/// (aleteo del murciélago, pasos del enemigo terrestre, fuego de una zona peligrosa, fuente de agua...).
/// - Lo conecta automáticamente al grupo Efectos del mixer.
/// - Lo pausa mientras el juego está en pausa (Time.timeScale = 0).
/// - Empieza en un punto aleatorio del clip para que varios enemigos iguales no suenen sincronizados.
/// - Aplica el volumen de efectos en WebGL, donde el mixer no lo controla.
/// Si el objeto se desactiva (enemigo oculto, peligro limpiado), el sonido se apaga solo.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FuenteSonidoBucle : MonoBehaviour
{
    [Tooltip("Pausa el sonido mientras el juego está en pausa.")]
    [SerializeField] private bool pausarConJuego = true;

    [Tooltip("Empieza el clip en un punto al azar (útil cuando hay varios objetos iguales).")]
    [SerializeField] private bool inicioAleatorio = true;

    private AudioSource fuente;
    private float volumenBase;
    private bool pausadoPorJuego;

    private void Awake()
    {
        fuente = GetComponent<AudioSource>();
        volumenBase = fuente.volume;
    }

    private void Start()
    {
        if (fuente.outputAudioMixerGroup == null)
            fuente.outputAudioMixerGroup = ReproductorAudio.GrupoEfectos;

        if (inicioAleatorio && fuente.clip != null && fuente.isPlaying)
            fuente.time = Random.Range(0f, fuente.clip.length);
    }

    private void Update()
    {
        fuente.volume = volumenBase * ReproductorAudio.FactorEfectos;

        if (!pausarConJuego) return;

        if (Time.timeScale <= 0f && fuente.isPlaying)
        {
            fuente.Pause();
            pausadoPorJuego = true;
        }
        else if (Time.timeScale > 0f && pausadoPorJuego)
        {
            fuente.UnPause();
            pausadoPorJuego = false;
        }
    }
}
