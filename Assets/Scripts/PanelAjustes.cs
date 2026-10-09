using UnityEngine;
using UnityEngine.UI;


public class PanelAjustes : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderEfectos;
    [Tooltip("Un Toggle normal de Unity UI sirve como botón de pantalla completa: " +
             "muestra el estado (marcado/desmarcado) sin necesitar texto dinámico.")]
    [SerializeField] private Toggle togglePantallaCompleta;

    private void OnEnable()
    {
        GestorAudio gestor = GestorAudio.Obtener();
        if (gestor == null) return;

        if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(gestor.VolumenMusicaActual);
        if (sliderEfectos != null) sliderEfectos.SetValueWithoutNotify(gestor.VolumenEfectosActual);
        if (togglePantallaCompleta != null) togglePantallaCompleta.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    public void OnCambioVolumenMusica(float valor)
    {
        GestorAudio.Obtener()?.EstablecerVolumenMusica(valor);
    }

    public void OnCambioVolumenEfectos(float valor)
    {
        GestorAudio.Obtener()?.EstablecerVolumenEfectos(valor);
    }

    public void OnCambioPantallaCompleta(bool activa)
    {
        GestorAudio.Obtener()?.EstablecerPantallaCompleta(activa);
    }

    public void Cerrar()
    {
        gameObject.SetActive(false);
    }
}