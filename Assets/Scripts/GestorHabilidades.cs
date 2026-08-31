using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public class GestorHabilidades : MonoBehaviour
{
    [Header("Habilidades - IMPORTANTE: respeta este orden")]
    [Tooltip("Elemento 0 = Cortar maleza (nivel 1) · 1 = Regar (nivel 2) · 2 = Recolectar basura (nivel 3)")]
    [SerializeField] private Habilidad[] habilidades = new Habilidad[3];

    [Header("Eventos (para la UI del menú)")]
    public EventoCambioHabilidad AlEquiparHabilidad;
    public EventoEntero AlDesbloquearHabilidad;

    private int indiceActual = -1; // -1 = ninguna equipada (solo ataque base)
    private bool manteniendoActivar = false;

    public Habilidad HabilidadActual => indiceActual >= 0 ? habilidades[indiceActual] : null;
    public int IndiceActual => indiceActual;

    private void Start()
    {
        foreach (Habilidad habilidad in habilidades)
        {
            if (habilidad != null) habilidad.Inicializar(transform);
        }
    }

    private void Update()
    {
        // El botón de habilidad puede mantenerse presionado (ej. cargar la esfera
        // de agua), así que se consulta cada frame mientras siga abajo.
        if (manteniendoActivar && HabilidadActual != null)
        {
            HabilidadActual.AlMantenerActivar();
        }
    }

    // --- Engancha estos 3 métodos desde el componente Player Input del jugador ---
    // (Behavior = "Invoke Unity Events"), en las acciones HabilidadSiguiente,
    // HabilidadAnterior y ActivarHabilidad respectivamente. Ver la guía de instalación.

    public void enHabilidadSiguiente(InputAction.CallbackContext contexto)
    {
        if (contexto.performed) CambiarHabilidad(1);
    }

    public void enHabilidadAnterior(InputAction.CallbackContext contexto)
    {
        if (contexto.performed) CambiarHabilidad(-1);
    }

    public void enActivarHabilidad(InputAction.CallbackContext contexto)
    {
        if (contexto.started)
        {
            manteniendoActivar = true;
            HabilidadActual?.AlPresionarActivar();
        }
        else if (contexto.canceled)
        {
            manteniendoActivar = false;
            HabilidadActual?.AlSoltarActivar();
        }
    }

    private void CambiarHabilidad(int direccion)
    {
        // -1 representa el estado neutro: ninguna habilidad equipada, solo
        // el ataque base disponible. Se agrega como una opción más del ciclo,
        // entre la última habilidad desbloqueada y la primera.
        List<int> opciones = new List<int> { -1 };
        for (int i = 0; i < habilidades.Length; i++)
        {
            if (habilidades[i] != null && habilidades[i].estaDesbloqueada)
                opciones.Add(i);
        }

        if (opciones.Count <= 1) return; // ninguna habilidad desbloqueada todavía

        int posicionActual = opciones.IndexOf(indiceActual);
        if (posicionActual < 0) posicionActual = 0;

        int siguientePosicion = (posicionActual + direccion + opciones.Count) % opciones.Count;
        EquiparHabilidad(opciones[siguientePosicion]);
    }

    private void EquiparHabilidad(int indice)
    {
        if (indice == indiceActual) return;

        HabilidadActual?.AlDesequipar();
        indiceActual = indice;
        HabilidadActual?.AlEquipar();

        AlEquiparHabilidad?.Invoke(indiceActual, HabilidadActual);
    }

    /// <summary>
    /// Llama a esto desde tu sistema de progresión de niveles, por ejemplo:
    /// gestorHabilidades.DesbloquearHabilidad(0); al completar el nivel 1.
    /// (También puedes usar el componente DesbloqueadorHabilidad.cs)
    /// </summary>
    public void DesbloquearHabilidad(int indice)
    {
        if (indice < 0 || indice >= habilidades.Length || habilidades[indice] == null) return;
        if (habilidades[indice].estaDesbloqueada) return;

        habilidades[indice].estaDesbloqueada = true;
        AlDesbloquearHabilidad?.Invoke(indice);
        EquiparHabilidad(indice); // la equipa automáticamente al desbloquearla
    }

    public bool EstaDesbloqueada(int indice)
    {
        if (indice < 0 || indice >= habilidades.Length || habilidades[indice] == null) return false;
        return habilidades[indice].estaDesbloqueada;
    }

    public Habilidad ObtenerHabilidad(int indice)
    {
        return (indice >= 0 && indice < habilidades.Length) ? habilidades[indice] : null;
    }
}