using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Sonido de un botón o toggle: al hacer clic y al pasar el mouse encima.
/// Normalmente lo agrega SonidosBotones automáticamente; también se puede
/// poner a mano en un botón concreto para darle un sonido distinto.
/// Funciona con el juego en pausa.
/// </summary>
public class SonidoBoton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private SonidoAleatorio sonidoClic = new SonidoAleatorio();
    [SerializeField] private SonidoAleatorio sonidoResaltar = new SonidoAleatorio();

    private Selectable elemento;

    private void Awake()
    {
        elemento = GetComponent<Selectable>();

        if (elemento is Button boton) boton.onClick.AddListener(ReproducirClic);
        else if (elemento is Toggle toggle) toggle.onValueChanged.AddListener(AlCambiarToggle);
    }

    private void OnDestroy()
    {
        if (elemento is Button boton) boton.onClick.RemoveListener(ReproducirClic);
        else if (elemento is Toggle toggle) toggle.onValueChanged.RemoveListener(AlCambiarToggle);
    }

    /// <summary>Lo llama SonidosBotones para asignar los sonidos compartidos del Canvas.</summary>
    public void Configurar(SonidoAleatorio clic, SonidoAleatorio resaltar)
    {
        // Si se puso a mano con clips propios, no se sobrescribe.
        if (sonidoClic.TieneClips || sonidoResaltar.TieneClips) return;

        sonidoClic = clic;
        sonidoResaltar = resaltar;
    }

    public void OnPointerEnter(PointerEventData datos)
    {
        if (elemento != null && !elemento.IsInteractable()) return;
        ReproductorAudio.Reproducir2D(sonidoResaltar);
    }

    private void ReproducirClic()
    {
        ReproductorAudio.Reproducir2D(sonidoClic);
    }

    private void AlCambiarToggle(bool valor)
    {
        ReproducirClic();
    }
}
