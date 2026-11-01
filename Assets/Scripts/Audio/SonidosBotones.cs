using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Da sonido a TODOS los botones y toggles de un Canvas de una sola vez.
/// Colócalo en la raíz de cada Canvas con botones (menú principal, pausa, ajustes,
/// game over, créditos, panel de habilidad...). Al iniciar agrega un SonidoBoton
/// a cada Button/Toggle hijo, incluidos los que están en paneles ocultos.
/// </summary>
public class SonidosBotones : MonoBehaviour
{
    [Tooltip("Sonido al hacer clic.")]
    [SerializeField] private SonidoAleatorio sonidoClic = new SonidoAleatorio();

    [Tooltip("Sonido al pasar el mouse encima (opcional).")]
    [SerializeField] private SonidoAleatorio sonidoResaltar = new SonidoAleatorio();

    private void Awake()
    {
        Selectable[] elementos = GetComponentsInChildren<Selectable>(true);

        foreach (Selectable elemento in elementos)
        {
            // Solo botones y toggles; los sliders y campos de texto sonarían a cada movimiento.
            if (!(elemento is Button) && !(elemento is Toggle)) continue;

            SonidoBoton sonido = elemento.GetComponent<SonidoBoton>();
            if (sonido == null) sonido = elemento.gameObject.AddComponent<SonidoBoton>();

            sonido.Configurar(sonidoClic, sonidoResaltar);
        }
    }
}
