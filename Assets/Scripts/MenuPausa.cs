using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla la pausa del juego y sus 4 opciones: reiniciar nivel, ajustes,
/// controles y salir al menú principal.
///
/// A diferencia de las habilidades, este script NO se engancha desde el
/// Player Input en el Inspector: el jugador es persistente entre escenas,
/// pero este menú vive en cada escena de nivel (se recrea cada vez), así
/// que se suscribe él mismo por código a la acción "Pausa" al cargar, y se
/// desuscribe al descargar la escena.
/// </summary>
public class MenuPausa : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelPausa;

    [Header("Reinicio")]
    [Tooltip("Id del PuntoAparicion donde debe reaparecer el jugador al reiniciar este nivel")]
    [SerializeField] private string idPuntoAparicionReinicio = "Entrada";

    [Header("Salida")]
    [SerializeField] private string nombreEscenaMenuPrincipal = "MenuInicio";

    private InputAction accionPausa;
    private bool estaPausado = false;

    private void Start()
    {
        JugadorPersistente jugador = JugadorPersistente.Obtener();
        if (jugador != null && jugador.InputJugador != null)
        {
            accionPausa = jugador.InputJugador.actions.FindAction("Pausa");
            if (accionPausa != null) accionPausa.performed += OnPausaPerformed;
        }
    }

    private void OnDestroy()
    {
        if (accionPausa != null) accionPausa.performed -= OnPausaPerformed;
    }

    private void OnPausaPerformed(InputAction.CallbackContext contexto)
    {
        AlternarPausa();
    }

    public void AlternarPausa()
    {
        estaPausado = !estaPausado;

        if (panelPausa != null) panelPausa.SetActive(estaPausado);
        Time.timeScale = estaPausado ? 0f : 1f;

        // El script de cámara bloquea el cursor una sola vez al iniciar; hay
        // que liberarlo aquí para poder hacer clic en los botones del menú.
        Cursor.lockState = estaPausado ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = estaPausado;
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
