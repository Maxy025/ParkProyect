using UnityEngine;

/// <summary>
/// Portal que teletransporta al jugador a otra escena al tocarlo. Colócalo
/// en un Collider marcado Is Trigger: uno por cada nivel dentro del hub, y
/// uno dentro de cada nivel para regresar al hub.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PortalDeNivel : MonoBehaviour
{
    [Tooltip("Nombre EXACTO de la escena de destino (debe estar en Build Settings)")]
    [SerializeField] private string nombreEscenaDestino;

    [Tooltip("Debe coincidir con el Id de un PuntoAparicion dentro de la escena de destino")]
    [SerializeField] private string idPuntoAparicionDestino = "Entrada";

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GestorNiveles.CargarNivel(nombreEscenaDestino, idPuntoAparicionDestino);
    }
}
