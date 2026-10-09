using UnityEngine;
using UnityEngine.InputSystem;

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
    public void Reubicar(Vector3 posicion, Quaternion rotacion)
    {
        if (controladorPersonaje != null) controladorPersonaje.enabled = false;

        transform.SetPositionAndRotation(posicion, rotacion);

        if (controladorPersonaje != null) controladorPersonaje.enabled = true;
    }

    public static JugadorPersistente Obtener() => instancia;
    public static void DestruirInstancia()
    {
        if (instancia == null) return;

        Destroy(instancia.gameObject);
        instancia = null;
    }
}