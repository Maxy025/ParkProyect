using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Guarda en memoria los objetivos completados durante la partida
/// (por ejemplo, "Hub_Portal1" o "Nivel1_Completado"). Al ser estático, se conserva
/// al cambiar de escena, así el progreso no se pierde al ir y volver del Hub.
/// Solo vive durante la sesión: no se guarda en disco.
/// </summary>
public static class ProgresoJuego
{
    // Ids de los niveles, para no escribirlos a mano en varios scripts.
    public const string Nivel1 = "Nivel1_Completado";
    public const string Nivel2 = "Nivel2_Completado";
    public const string Nivel3 = "Nivel3_Completado";

    private static readonly HashSet<string> objetivosCompletados = new HashSet<string>();

    /// <summary>
    /// Se invoca cuando un objetivo se completa por primera vez (envía su Id).
    /// Lo usa ActivadorPorProgreso para reaccionar sin tener que recargar la escena.
    /// </summary>
    public static event Action<string> AlCompletarObjetivo;

    /// <summary>Indica si un objetivo ya se completó.</summary>
    public static bool EstaCompletado(string idObjetivo)
    {
        return !string.IsNullOrEmpty(idObjetivo) && objetivosCompletados.Contains(idObjetivo);
    }

    /// <summary>Indica si TODOS los objetivos de la lista están completados.</summary>
    public static bool TodosCompletados(string[] idsObjetivos)
    {
        if (idsObjetivos == null) return true;

        foreach (string id in idsObjetivos)
            if (!string.IsNullOrEmpty(id) && !objetivosCompletados.Contains(id))
                return false;

        return true;
    }

    /// <summary>Marca un objetivo como completado.</summary>
    public static void MarcarCompletado(string idObjetivo)
    {
        if (string.IsNullOrEmpty(idObjetivo)) return;

        // HashSet.Add devuelve false si ya existía: así el evento se invoca una sola vez.
        if (objetivosCompletados.Add(idObjetivo))
            AlCompletarObjetivo?.Invoke(idObjetivo);
    }

    /// <summary>Borra todo el progreso (se llama al iniciar una partida nueva).</summary>
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
        AlCompletarObjetivo = null;
    }
}
