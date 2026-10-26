using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoJugador : MonoBehaviour, IVelocidadModificable
{
    [SerializeField] private Transform cameraTransform; // Referencia al transform de la cámara para orientar el movimiento del jugador
    [SerializeField] private float velocidadMovimiento = 5f; // Velocidad de movimiento del jugador
    [SerializeField] private float alturaSalto = 2f; // Velocidad de de salto del jugador
    [SerializeField] private float gravedad = -9.8f; // Gravedad aplicada al jugador
    [SerializeField] private bool direccionCara = false; // Variable para determinar si el jugador debe mirar en la dirección del movimiento

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private bool isGrounded;

    private CharacterController characterController; // Componente CharacterController del jugador
    private Vector2 movimientoInput; // Vector para almacenar el input de movimiento
    private Vector3 velocidad; // Vector para almacenar la velocidad actual del jugador

    // --- Añadido para el sistema de habilidades ---
    // Permite que HabilidadRecolectarBasura (u otra habilidad futura) reduzca
    // temporalmente la velocidad de movimiento sin tocar el resto del script.

    // Animaciones
    private Animator animator;

    // ------- //
    private float multiplicadorVelocidad = 1f;

    public void EstablecerMultiplicadorVelocidad(float multiplicador)
    {
        multiplicadorVelocidad = multiplicador;
    }
    // --- Fin de lo añadido ---

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
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
            Debug.Log("Estoy en el aire");
            animator.SetTrigger("Salto");
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

        if (direccionCara && direccionMovimiento.sqrMagnitude > 0.001F) // Si la opción de dirección cara está activada y el jugador se está moviendo  
        {
            Quaternion rotacionObetivo = Quaternion.LookRotation(direccionMovimiento, Vector3.up); // Calcular la rotación objetivo para que el jugador mire en la dirección del movimiento
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObetivo, Time.deltaTime * 10f); // Interpolar suavemente la rotación del jugador hacia la rotación objetivo

            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, (int)whatIsGround);

        if (isGrounded) 
        {
            animator.SetBool("enSuelo", true);
        }
        else
        {
            animator.SetBool("enSuelo", false);
        }        

        // Aplicar gravedad
        velocidad.y += gravedad * Time.deltaTime;

        // Combinar movimiento horizontal y vertical en un solo vector
        Vector3 movimientoTotal = (direccionMovimiento * velocidadMovimiento * multiplicadorVelocidad) + new Vector3(0, velocidad.y, 0);

        // Mover una sola vez por frame
        characterController.Move(movimientoTotal * Time.deltaTime);
    }
}