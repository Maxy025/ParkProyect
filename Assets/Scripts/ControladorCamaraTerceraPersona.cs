using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControladorCamaraTerceraPersona : MonoBehaviour
{
    [SerializeField] private float velocidadZoom = 2f; // Velocidad de zoom de la cámara
    [SerializeField] private float velocidadZoomLerp = 10f; // Velocidad de interpolación del zoom
    [SerializeField] private float distanciaMinima = 2f; // Distancia mínima de la cámara al jugador
    [SerializeField] private float distanciaMaxima = 15f; // Distancia máxima de la cámara al jugador


    private ControladorPersonaje controles;

    private CinemachineCamera camara;
    private CinemachineOrbitalFollow orbital;

    private Vector2 scrollDelta;

    private float targetZoom;
    private float zoomActual;

    void Start()
    {
        controles = new ControladorPersonaje();
        controles.Enable();
        controles.ControlCamara.ZoomMouse.performed += HandleMouseScroll;

        Cursor.lockState = CursorLockMode.Locked;

        camara = GetComponent<CinemachineCamera>();
        orbital = camara.GetComponent<CinemachineOrbitalFollow>();

        targetZoom = zoomActual = orbital.Radius;
    }

    private void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrollDelta = context.ReadValue<Vector2>();
        Debug.Log($"Scroll Delta. Value: {scrollDelta}");
    }

    // Update is called once per frame
    void Update()
    {
        if(scrollDelta.y != 0)
        {
            if (orbital != null)
            {
                targetZoom = Mathf.Clamp(orbital.Radius - scrollDelta.y * velocidadZoom, distanciaMinima, distanciaMaxima);
                scrollDelta = Vector2.zero; // Reiniciar el scroll delta después de usarlo
            }
        }
        float bumperDelta = controles.ControlCamara.GamepadZoom.ReadValue<float>();
        if (bumperDelta != 0)
        {
           targetZoom = Mathf.Clamp(orbital.Radius - bumperDelta * velocidadZoom, distanciaMinima, distanciaMaxima);
        }
        zoomActual = Mathf.Lerp(zoomActual, targetZoom, Time.deltaTime * velocidadZoomLerp);
        orbital.Radius = zoomActual;
    }
}
