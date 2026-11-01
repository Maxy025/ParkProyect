using UnityEngine;

/// <summary>
/// Sonidos del jugador: pasos, salto, aterrizaje, daño y muerte.
/// Va en la raíz del prefab del jugador (junto a MovimientoJugador y PlayerHealth).
/// No modifica MovimientoJugador ni PlayerHealth: lee el movimiento del CharacterController
/// y se suscribe por código a los eventos OnDamageTaken y OnPlayerDeath de PlayerHealth.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SonidosJugador : MonoBehaviour
{
    [Header("Pasos")]
    [Tooltip("Pon varios clips de pasos: se alternan al azar con tono variable.")]
    [SerializeField] private SonidoAleatorio sonidoPasos = new SonidoAleatorio();

    [Tooltip("Metros recorridos entre un paso y otro.")]
    [SerializeField, Min(0.1f)] private float distanciaEntrePasos = 1.6f;

    [Tooltip("Velocidad mínima (m/s) para que suenen los pasos.")]
    [SerializeField, Min(0f)] private float velocidadMinimaPasos = 0.5f;

    [Header("Salto y aterrizaje")]
    [SerializeField] private SonidoAleatorio sonidoSalto = new SonidoAleatorio();
    [SerializeField] private SonidoAleatorio sonidoAterrizaje = new SonidoAleatorio();

    [Tooltip("Velocidad hacia arriba (m/s) a partir de la cual se considera que el jugador saltó.")]
    [SerializeField, Min(0f)] private float velocidadMinimaSalto = 2f;

    [Tooltip("Segundos mínimos en el aire para que suene el aterrizaje (evita que suene en escalones).")]
    [SerializeField, Min(0f)] private float tiempoAireMinimo = 0.35f;

    [Header("Vida")]
    [SerializeField] private SonidoAleatorio sonidoDano = new SonidoAleatorio();
    [SerializeField] private SonidoAleatorio sonidoMuerte = new SonidoAleatorio();

    [Header("Espacialidad")]
    [Tooltip("0 = plano (recomendado para el jugador, porque la cámara siempre está cerca), 1 = 3D.")]
    [SerializeField, Range(0f, 1f)] private float mezclaEspacial = 0f;

    private CharacterController controlador;
    private PlayerHealth vida;

    private bool estabaEnSuelo = true;
    private float tiempoEnAire;
    private float distanciaAcumulada;
    private bool saltoSonado;

    private void Start()
    {
        controlador = GetComponent<CharacterController>();
        vida = GetComponent<PlayerHealth>();

        if (vida != null)
        {
            vida.OnDamageTaken.AddListener(ReproducirDano);
            vida.OnPlayerDeath.AddListener(ReproducirMuerte);
        }
    }

    private void OnDestroy()
    {
        if (vida != null)
        {
            vida.OnDamageTaken.RemoveListener(ReproducirDano);
            vida.OnPlayerDeath.RemoveListener(ReproducirMuerte);
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || controlador == null || !controlador.enabled) return;
        if (vida != null && vida.IsDead) return;

        bool enSuelo = controlador.isGrounded;
        Vector3 velocidad = controlador.velocity;

        // Salto: estaba en el suelo y de repente sube rápido.
        if (!enSuelo && !saltoSonado && velocidad.y > velocidadMinimaSalto)
        {
            Reproducir(sonidoSalto);
            saltoSonado = true;
        }

        if (enSuelo)
        {
            // Aterrizaje: vuelve al suelo después de estar un rato en el aire.
            if (!estabaEnSuelo && tiempoEnAire >= tiempoAireMinimo)
            {
                Reproducir(sonidoAterrizaje);
                distanciaAcumulada = 0f;
            }

            tiempoEnAire = 0f;
            saltoSonado = false;
            ActualizarPasos(velocidad);
        }
        else
        {
            tiempoEnAire += Time.deltaTime;
        }

        estabaEnSuelo = enSuelo;
    }

    private void ActualizarPasos(Vector3 velocidad)
    {
        float rapidez = new Vector3(velocidad.x, 0f, velocidad.z).magnitude;

        if (rapidez < velocidadMinimaPasos)
        {
            // Al detenerse se deja "medio paso" cargado para que el primero al arrancar suene rápido.
            distanciaAcumulada = distanciaEntrePasos * 0.5f;
            return;
        }

        distanciaAcumulada += rapidez * Time.deltaTime;
        if (distanciaAcumulada >= distanciaEntrePasos)
        {
            distanciaAcumulada = 0f;
            Reproducir(sonidoPasos);
        }
    }

    private void ReproducirDano()
    {
        Reproducir(sonidoDano);
    }

    private void ReproducirMuerte()
    {
        Reproducir(sonidoMuerte);
    }

    private void Reproducir(SonidoAleatorio sonido)
    {
        ReproductorAudio.ReproducirEn(sonido, transform.position, mezclaEspacial);
    }
}
