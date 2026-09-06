using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Muestra la pantalla de Game Over cuando el jugador muere. Se suscribe por
/// código al evento OnPlayerDeath de PlayerHealth (mismo motivo que
/// MenuPausa con el input): el jugador es persistente entre escenas, pero
/// esta pantalla vive en cada nivel y se recrea cada vez.
/// </summary>
public class PantallaGameOver : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelGameOver;

    [Header("Reinicio")]
    [Tooltip("Id del PuntoAparicion donde debe reaparecer el jugador al reiniciar este nivel")]
    [SerializeField] private string idPuntoAparicionReinicio = "Entrada";

    [Header("Salida")]
    [SerializeField] private string nombreEscenaMenuPrincipal = "MenuInicio";

    private PlayerHealth vidaJugador;

    private void Start()
    {
        JugadorPersistente jugador = JugadorPersistente.Obtener();
        if (jugador != null && jugador.VidaJugador != null)
        {
            vidaJugador = jugador.VidaJugador;
            vidaJugador.OnPlayerDeath.AddListener(Mostrar);
        }
    }

    private void OnDestroy()
    {
        if (vidaJugador != null)
            vidaJugador.OnPlayerDeath.RemoveListener(Mostrar);
    }

    public void Mostrar()
    {
        Time.timeScale = 0f;
        if (panelGameOver != null) panelGameOver.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GestorNiveles.CargarNivel(SceneManager.GetActiveScene().name, idPuntoAparicionReinicio, true);
    }

    public void SalirAMenuPrincipal()
    {
        Time.timeScale = 1f;
        GestorNiveles.SalirAMenuPrincipal(nombreEscenaMenuPrincipal);
    }
}
