using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Vida básica de un enemigo, compatible con el ataque base del jugador
/// (implementa IDanable). Puedes reemplazarla por tu propio sistema de
/// vida de enemigos siempre que también implemente IDanable.
/// </summary>
public class VidaEnemigo : MonoBehaviour, IDanable
{
    [SerializeField] private float vidaMaxima = 50f;
    public UnityEvent AlMorir;

    private float vidaActual;

    private void Awake() => vidaActual = vidaMaxima;

    public void RecibirDano(float cantidad)
    {
        if (vidaActual <= 0f) return;

        vidaActual -= cantidad;
        if (vidaActual <= 0f)
        {
            vidaActual = 0f;
            AlMorir?.Invoke();
            Destroy(gameObject);
        }
    }
}