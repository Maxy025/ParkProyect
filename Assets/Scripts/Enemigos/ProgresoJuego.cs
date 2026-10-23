using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Guarda en memoria los objetivos completados durante la partida
/// (por ejemplo, "Hub_Portal1"). Al ser estático, se conserva al cambiar de escena,
/// así el portal no se vuelve a bloquear al regresar al Hub.
/// </summary>
public static class ProgresoJuego
{
    private static readonly HashSet<string> objetivosCompletados = new HashSet<string>();

    /// <summary>Indica si un objetivo ya se completó.</summary>
    public static bool EstaCompletado(string idObjetivo)
    {
        return !string.IsNullOrEmpty(idObjetivo) && objetivosCompletados.Contains(idObjetivo);
    }

    /// <summary>Marca un objetivo como completado.</summary>
    public static void MarcarCompletado(string idObjetivo)
    {
        if (!string.IsNullOrEmpty(idObjetivo))
            objetivosCompletados.Add(idObjetivo);
    }

    /// <summary>Borra todo el progreso (llamar al iniciar una partida nueva).</summary>
    public static void Reiniciar()
    {
        objetivosCompletados.Clear();
    }

    // Limpia el progreso cada vez que se entra a Play en el editor,
    // aunque esté desactivada la recarga de dominio.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarAlIniciarJuego()
    {
        objetivosCompletados.Clear();
    }
}
