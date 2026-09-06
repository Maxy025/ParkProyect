using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Hace que el jugador persista entre escenas (DontDestroyOnLoad) y expone
/// referencias a sus componentes clave (PlayerInput, PlayerHealth) para que
/// otros sistemas (menú de pausa, Game Over) los encuentren de forma
/// dinámica, sin depender de una referencia arrastrada a mano en el
/// Inspector que se rompería si el jugador llegara a recrearse.
///
/// Coloca este componente en el objeto raíz del jugador. Debe existir
/// EXACTAMENTE UNA instancia del jugador en toda la partida: colócala
/// directamente en la escena del Hub (donde entra por primera vez desde el
/// menú de inicio) y NO la repitas en las escenas de los niveles — si por
/// error queda una copia ahí, se autodestruye sola al cargar.
/// </summary>
public class JugadorPersistente : MonoBehaviour
{
    private static JugadorPersistente instancia;

    public PlayerInput InputJugador { get; private set; }
    public PlayerHealth VidaJugador { get; private set; }
    public GestorHabilidades Habilidades { get; private set; }

    private CharacterController controladorPersonaje;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            // Ya existe un jugador persistente (ej. volviste al Hub desde un
            // nivel y esta escena trae su propia copia) — esta sobra.
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);

        InputJugador = GetComponent<PlayerInput>();
        VidaJugador = GetComponent<PlayerHealth>();
        Habilidades = GetComponent<GestorHabilidades>();
        controladorPersonaje = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Mueve al jugador a una posición/rotación nueva de forma segura.
    /// Desactivar el CharacterController un instante es necesario porque,
    /// si sigue activo, ignora cambios directos de transform.position.
    /// </summary>
    public void Reubicar(Vector3 posicion, Quaternion rotacion)
    {
        if (controladorPersonaje != null) controladorPersonaje.enabled = false;

        transform.SetPositionAndRotation(posicion, rotacion);

        if (controladorPersonaje != null) controladorPersonaje.enabled = true;
    }

    public static JugadorPersistente Obtener() => instancia;

    /// <summary>
    /// Destruye al jugador persistente por completo. Llama a esto antes de
    /// volver al menú principal, para que la próxima partida empiece de
    /// cero (vida y posición limpias) en vez de arrastrar el jugador de la
    /// sesión anterior.
    /// </summary>
    public static void DestruirInstancia()
    {
        if (instancia == null) return;

        Destroy(instancia.gameObject);
        instancia = null;
    }
}