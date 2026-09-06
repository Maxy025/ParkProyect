using UnityEngine;

/// <summary>
/// Marca un punto de aparición del jugador. Colócalo donde quieras que el
/// jugador aparezca al llegar a esta escena por un portal específico.
/// El Id debe coincidir con el "Id Punto Aparicion Destino" del PortalDeNivel
/// (u otro sistema) que apunta hacia esta escena.
/// </summary>
public class PuntoAparicion : MonoBehaviour
{
    [SerializeField] private string id = "Entrada";
    public string Id => id;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);
    }
}
