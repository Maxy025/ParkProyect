using UnityEngine;

/// <summary>
/// Colócalo en plantas, semillas o mecanismos que deban reaccionar al ser
/// regados con la habilidad de agua. Ej: hacer crecer una plataforma,
/// abrir una flor que bloquea el paso, nutrir un árbol, etc.
/// </summary>
public class Regable : MonoBehaviour
{
    [Tooltip("Cantidad mínima de carga de agua (0-1) necesaria para activar el efecto")]
    [SerializeField, Range(0f, 1f)] private float cantidadAguaRequerida = 0.3f;
    [SerializeField] private bool activarUnaVez = true;

    [Tooltip("Se invoca con la cantidad de agua recibida (0-1). Engancha aquí, por ejemplo, " +
             "el método Crecer() de PlataformaCreciente.cs")]
    public EventoFlotante AlRegar;

    private bool yaActivado = false;

    public void Regar(float cantidad)
    {
        if (activarUnaVez && yaActivado) return;
        if (cantidad < cantidadAguaRequerida) return;

        yaActivado = true;
        AlRegar?.Invoke(cantidad);
    }
}
