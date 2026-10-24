using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Panel único de la escena que muestra el contenido de los CartelTutorial.
/// - Muestra el aviso "Presiona F para leer" cuando el jugador está junto a un cartel.
/// - Al presionar el botón de interactuar, abre el mensaje (y congela el juego).
/// - Al presionarlo otra vez (o con el botón Cerrar), lo cierra.
///
/// Igual que MenuPausa, se suscribe por código a la acción del Input System
/// ("Interactuar"), porque el jugador es persistente y este panel vive en la escena.
/// Si la acción no existe, usa la tecla de respaldo.
/// </summary>
public class PanelTutorial : MonoBehaviour
{
    public static PanelTutorial Instancia { get; private set; }

    [Header("Panel del mensaje")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoMensaje;

    [Tooltip("Texto del objetivo (opcional). Se oculta si el cartel no tiene objetivo.")]
    [SerializeField] private TMP_Text textoObjetivo;

    [Tooltip("{0} = objetivo del cartel.")]
    [SerializeField] private string formatoObjetivo = "Objetivo: {0}";

    [Header("Aviso de interacción")]
    [Tooltip("Objeto de UI con el aviso 'Presiona F para leer'. Se muestra solo junto a un cartel.")]
    [SerializeField] private GameObject avisoInteractuar;

    [Header("Entrada")]
    [Tooltip("Nombre EXACTO de la acción en el Input Actions del jugador.")]
    [SerializeField] private string nombreAccion = "Interactuar";

    [Tooltip("Tecla que se usa si la acción no existe en el Input Actions.")]
    [SerializeField] private Key teclaRespaldo = Key.F;

    [Header("Comportamiento")]
    [Tooltip("Congela el juego y libera el cursor mientras el mensaje está abierto.")]
    [SerializeField] private bool pausarAlLeer = true;

    private InputAction accionInteractuar;
    private CartelTutorial cartelCercano;
    private bool abierto = false;

    public bool EstaAbierto => abierto;

    private void Awake()
    {
        Instancia = this;

        if (panel != null) panel.SetActive(false);
        if (avisoInteractuar != null) avisoInteractuar.SetActive(false);
    }

    private void Start()
    {
        JugadorPersistente jugador = JugadorPersistente.Obtener();
        if (jugador != null && jugador.InputJugador != null)
            accionInteractuar = jugador.InputJugador.actions.FindAction(nombreAccion);

        if (accionInteractuar != null)
            accionInteractuar.performed += AlPresionarInteractuar;
        else
            Debug.LogWarning($"[PanelTutorial] No se encontró la acción '{nombreAccion}'. Se usará la tecla {teclaRespaldo}.", this);
    }

    private void OnDestroy()
    {
        if (accionInteractuar != null)
            accionInteractuar.performed -= AlPresionarInteractuar;

        if (Instancia == this) Instancia = null;
    }

    private void Update()
    {
        // Respaldo por si no existe la acción en el Input Actions.
        if (accionInteractuar == null && Keyboard.current != null && Keyboard.current[teclaRespaldo].wasPressedThisFrame)
            Interactuar();
    }

    private void AlPresionarInteractuar(InputAction.CallbackContext contexto)
    {
        Interactuar();
    }

    /// <summary>Abre el mensaje del cartel cercano o cierra el que está abierto.</summary>
    public void Interactuar()
    {
        if (abierto)
        {
            Cerrar();
            return;
        }

        // No abrir si otro menú (pausa, game over, panel de habilidad) ya congeló el juego.
        if (cartelCercano != null && Time.timeScale > 0f)
            Mostrar(cartelCercano);
    }

    public void Mostrar(CartelTutorial cartel)
    {
        if (cartel == null) return;

        if (textoTitulo != null) textoTitulo.text = cartel.Titulo;
        if (textoMensaje != null) textoMensaje.text = cartel.Mensaje;

        if (textoObjetivo != null)
        {
            bool tieneObjetivo = !string.IsNullOrEmpty(cartel.Objetivo);
            textoObjetivo.gameObject.SetActive(tieneObjetivo);
            if (tieneObjetivo) textoObjetivo.text = string.Format(formatoObjetivo, cartel.Objetivo);
        }

        abierto = true;
        if (panel != null) panel.SetActive(true);
        ActualizarAviso();
        cartel.MarcarLeido();

        if (pausarAlLeer)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>Cierra el mensaje. Engánchalo también al botón "Cerrar" del panel.</summary>
    public void Cerrar()
    {
        if (!abierto) return;

        abierto = false;
        if (panel != null) panel.SetActive(false);
        ActualizarAviso();

        if (pausarAlLeer)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RegistrarCartelCercano(CartelTutorial cartel)
    {
        cartelCercano = cartel;
        ActualizarAviso();
    }

    public void QuitarCartelCercano(CartelTutorial cartel)
    {
        if (cartelCercano != cartel) return;

        cartelCercano = null;
        ActualizarAviso();
    }

    private void ActualizarAviso()
    {
        if (avisoInteractuar != null)
            avisoInteractuar.SetActive(cartelCercano != null && !abierto);
    }
}
