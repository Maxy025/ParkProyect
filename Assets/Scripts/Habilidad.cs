using UnityEngine;

/// <summary>
/// Clase base para las 3 habilidades intercambiables del jugador.
/// Hereda de esta clase y sobreescribe los métodos que necesites.
/// GestorHabilidades se encarga de llamar a estos métodos en el momento correcto.
/// </summary>
public abstract class Habilidad : MonoBehaviour
{
    [Header("Datos generales")]
    public string nombreHabilidad = "Habilidad";
    public Sprite icono;

    /// <summary>Actualizado automáticamente por GestorHabilidades. No lo cambies a mano.</summary>
    [HideInInspector] public bool estaDesbloqueada = false;

    protected Transform jugador;

    public virtual void Inicializar(Transform transformJugador)
    {
        jugador = transformJugador;
    }

    /// <summary>Se llama al equipar esta habilidad (por desbloqueo o al cambiar con Q/E).</summary>
    public virtual void AlEquipar() { }

    /// <summary>Se llama al cambiar a otra habilidad.</summary>
    public virtual void AlDesequipar() { }

    /// <summary>Botón de habilidad presionado (un solo frame).</summary>
    public virtual void AlPresionarActivar() { }

    /// <summary>Botón de habilidad mantenido presionado (cada frame mientras se sostiene).</summary>
    public virtual void AlMantenerActivar() { }

    /// <summary>Botón de habilidad soltado.</summary>
    public virtual void AlSoltarActivar() { }
}
