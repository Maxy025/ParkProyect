using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el panel de ajustes: sliders de música/efectos y el toggle de
/// pantalla completa. Al mostrarse, sincroniza los controles con los valores
/// actuales guardados en GestorAudio (para que no se vean en 0 si el jugador
/// ya había ajustado el volumen antes).
/// </summary>
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

        // SetValueWithoutNotify evita que, al sincronizar los sliders con los
        // valores guardados, se disparen sus propios eventos OnValueChanged.
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