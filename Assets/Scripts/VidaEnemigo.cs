using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Vida básica de un enemigo, compatible con el ataque base del jugador
/// (implementa IDanable). Al recibir daño alerta a la IA para que persiga
/// al jugador, y al morir invoca AlMorir una sola vez (lo usa ObjetivoEnemigosVencidos).
/// </summary>
public class VidaEnemigo : MonoBehaviour, IDanable
{
    [Tooltip("Vida con la que empieza el enemigo.")]
    [SerializeField] private float vidaMaxima = 50f;

    [Tooltip("Si se activa, al recibir daño el enemigo reacciona: persigue al jugador y retrocede (se configura en su IA).")]
    [SerializeField] private bool alertarAlRecibirDano = true;

    [Header("Sonidos")]
    [Tooltip("Sonido al recibir un golpe (sin morir).")]
    [SerializeField] private SonidoAleatorio sonidoDano = new SonidoAleatorio();
    [Tooltip("Sonido al morir. Sigue sonando aunque el enemigo se destruya.")]
    [SerializeField] private SonidoAleatorio sonidoMuerte = new SonidoAleatorio();

    [Header("Eventos")]
    [Tooltip("Se invoca cada vez que el enemigo recibe daño sin morir (lo usa EfectoDanoEnemigo para parpadear en rojo).")]
    public UnityEvent AlRecibirDano = new UnityEvent();

    [Tooltip("Se invoca una sola vez cuando el enemigo muere.")]
    public UnityEvent AlMorir = new UnityEvent();

    private float vidaActual;
    private EnemigoIA ia;

    /// <summary>Vida actual (solo lectura).</summary>
    public float VidaActual => vidaActual;

    /// <summary>Indica si el enemigo ya murió.</summary>
    public bool EstaMuerto => vidaActual <= 0f;

    private void Awake()
    {
        vidaActual = vidaMaxima;
        ia = GetComponent<EnemigoIA>();
    }

    public void RecibirDano(float cantidad)
    {
        // Si ya murió no recibe más daño (evita contar dos veces al mismo enemigo).
        if (EstaMuerto) return;

        vidaActual -= cantidad;

        if (vidaActual <= 0f)
        {
            vidaActual = 0f;
            ReproductorAudio.ReproducirEn(sonidoMuerte, transform.position);
            AlMorir?.Invoke();
            Destroy(gameObject);
            return;
        }

        ReproductorAudio.ReproducirEn(sonidoDano, transform.position);
        AlRecibirDano?.Invoke();

        // Reacción al golpe: persigue al jugador y retrocede (cancela su ataque).
        if (alertarAlRecibirDano && ia != null)
            ia.RecibirGolpe();
    }
}