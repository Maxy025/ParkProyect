using UnityEngine;

/// <summary>
/// Comodín para poner un sonido en cualquier UnityEvent desde el Inspector, sin escribir código.
/// Ejemplo: en MetaNivel > AlLlegarMeta, arrastra este objeto y elige SonidoEnEvento > Reproducir().
/// Sirve para cualquier evento (AlCortar, AlRegar, AlMorir, AlCambiarConteo, botones...).
/// </summary>
public class SonidoEnEvento : MonoBehaviour
{
    [SerializeField] private SonidoAleatorio sonido = new SonidoAleatorio();

    [Tooltip("Activado: suena en la posición de este objeto (más bajo al alejarse). " +
             "Desactivado: suena plano, igual en todos lados (interfaz, jingles).")]
    [SerializeField] private bool espacial = false;

    /// <summary>Reproduce el sonido. Engánchalo a cualquier evento.</summary>
    public void Reproducir()
    {
        if (espacial) ReproductorAudio.ReproducirEn(sonido, transform.position);
        else ReproductorAudio.Reproducir2D(sonido);
    }
}
