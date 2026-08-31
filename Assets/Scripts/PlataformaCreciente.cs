using UnityEngine;
using System.Collections;

/// <summary>
/// Ejemplo de efecto positivo del agua: la plataforma crece desde el suelo
/// hasta su tamaño final. Engancha el método Crecer() al evento AlRegar
/// de un componente Regable (en el mismo objeto o uno cercano) desde el Inspector.
/// </summary>
public class PlataformaCreciente : MonoBehaviour
{
    [SerializeField] private Vector3 escalaFinal = Vector3.one;
    [SerializeField] private float duracionCrecimiento = 1.5f;
    [SerializeField] private AnimationCurve curvaCrecimiento = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void Awake()
    {
        transform.localScale = new Vector3(escalaFinal.x, 0f, escalaFinal.z);
    }

    // La firma (float) coincide con EventoFlotante para conectarse directo a Regable.AlRegar
    public void Crecer(float cantidadAgua)
    {
        StopAllCoroutines();
        StartCoroutine(RutinaCrecimiento());
    }

    private IEnumerator RutinaCrecimiento()
    {
        Vector3 escalaInicial = transform.localScale;
        float transcurrido = 0f;

        while (transcurrido < duracionCrecimiento)
        {
            transcurrido += Time.deltaTime;
            float t = curvaCrecimiento.Evaluate(transcurrido / duracionCrecimiento);
            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);
            yield return null;
        }

        transform.localScale = escalaFinal;
    }
}
