using UnityEngine;

/// <summary>
/// Detector de contacto de las tijeras. Colócalo en el mismo GameObject que
/// tenga el Collider (marcado como Is Trigger) de las tijeras del jugador.
/// Debe ser hijo del objeto "modeloTijeras" asignado en HabilidadCortarMaleza,
/// para que solo pueda cortar mientras esa habilidad esté equipada.
///
/// Necesita un Rigidbody (Kinematic): Unity solo genera eventos de trigger
/// (OnTriggerEnter) si AL MENOS UNO de los dos colliders involucrados tiene
/// un Rigidbody. Ni este collider ni el de la Maleza lo tienen por defecto,
/// así que sin esto el corte nunca se dispara. Se agrega y configura solo
/// al añadir el componente por primera vez.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CortadorMaleza : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // no debe caer ni ser empujado por la física
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Maleza maleza))
        {
            maleza.Cortar();
        }
    }
}