using UnityEngine;

/// <summary>
/// Ayudante simple para desbloquear una habilidad desde tu sistema de niveles.
/// Puedes usarlo como un trigger de "fin de nivel" (desbloquearAlEntrarTrigger),
/// o colocarlo en la escena del nivel 3 para desbloquear la habilidad 3
/// automáticamente al empezar (desbloquearAlInicio).
/// </summary>
public class DesbloqueadorHabilidad : MonoBehaviour
{
    [SerializeField] private GestorHabilidades gestorHabilidades;

    [Tooltip("0 = Cortar maleza (fin nivel 1) · 1 = Regar (fin nivel 2) · 2 = Basura (inicio nivel 3)")]
    [SerializeField] private int indiceHabilidadADesbloquear;

    [Tooltip("Desbloquea automáticamente al cargar la escena (ideal para la habilidad 3 al iniciar el nivel 3)")]
    [SerializeField] private bool desbloquearAlInicio = false;

    [Tooltip("Desbloquea cuando el jugador entra en este Collider (Is Trigger), ideal para el final de un nivel")]
    [SerializeField] private bool desbloquearAlEntrarTrigger = false;

    private void Start()
    {
        if (desbloquearAlInicio) Desbloquear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!desbloquearAlEntrarTrigger) return;
        if (!other.CompareTag("Player")) return;
        Desbloquear();
    }

    public void Desbloquear()
    {
        if (gestorHabilidades != null)
            gestorHabilidades.DesbloquearHabilidad(indiceHabilidadADesbloquear);
    }
}
