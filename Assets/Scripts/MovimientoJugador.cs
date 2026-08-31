using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoJugador : MonoBehaviour, IVelocidadModificable
{
    [SerializeField] private Transform cameraTransform; // Referencia al transform de la cámara para orientar el movimiento del jugador
    [SerializeField] private float velocidadMovimiento = 5f; // Velocidad de movimiento del jugador
    [SerializeField] private float alturaSalto = 2f; // Velocidad de de salto del jugador
    [SerializeField] private float gravedad = -9.8f; // Gravedad aplicada al jugador
    [SerializeField] private bool direccionCara = false; // Variable para determinar si el jugador debe mirar en la dirección del movimiento

    private CharacterController characterController; // Componente CharacterController del jugador
    private Vector2 movimientoInput; // Vector para almacenar el input de movimiento
    private Vector3 velocidad; // Vector para almacenar la velocidad actual del jugador

    // --- Añadido para el sistema de habilidades ---
    // Permite que HabilidadRecolectarBasura (u otra habilidad futura) reduzca
    // temporalmente la velocidad de movimiento sin tocar el resto del script.
    private float multiplicadorVelocidad = 1f;

    public void EstablecerMultiplicadorVelocidad(float multiplicador)
    {
        multiplicadorVelocidad = multiplicador;
    }
    // --- Fin de lo añadido ---

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void enMovimiento(InputAction.CallbackContext context)
    {
        movimientoInput = context.ReadValue<Vector2>();
    }

    public void enSalto(InputAction.CallbackContext context)
    {
       
        if (context.performed && characterController.isGrounded)
        {
            velocidad.y = Mathf.Sqrt(alturaSalto * -2f * gravedad);
        }
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 delante = cameraTransform.forward; // Obtener la dirección hacia adelante de la cámara
        Vector3 derecha = cameraTransform.right; // Obtener la dirección hacia la derecha de la cámara

        delante.y = 0; // Eliminar la componente vertical para que el movimiento sea horizontal
        derecha.y = 0; // Eliminar la componente vertical para que el movimiento sea horizontal
        
        delante.Normalize(); // Normalizar la dirección hacia adelante
        derecha.Normalize(); // Normalizar la dirección hacia la derecha
         
        Vector3 direccionMovimiento = delante * movimientoInput.y + derecha * movimientoInput.x; // Calcular la dirección de movimiento basada en el input y la orientación de la cámara
        characterController.Move(direccionMovimiento * velocidadMovimiento * multiplicadorVelocidad * Time.deltaTime); // Mover al jugador según la dirección de movimiento, la velocidad de movimiento y el multiplicador de carga

        if(direccionCara && direccionMovimiento.sqrMagnitude > 0.001F) // Si la opción de dirección cara está activada y el jugador se está moviendo  
        {
            Quaternion rotacionObetivo = Quaternion.LookRotation(direccionMovimiento, Vector3.up); // Calcular la rotación objetivo para que el jugador mire en la dirección del movimiento
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObetivo, Time.deltaTime * 10f); // Interpolar suavemente la rotación del jugador hacia la rotación objetivo
        }


        velocidad.y += gravedad * Time.deltaTime; // Aplicar gravedad al jugador
        characterController.Move(velocidad * Time.deltaTime); // Mover al jugador según la velocidad actual
    }
}