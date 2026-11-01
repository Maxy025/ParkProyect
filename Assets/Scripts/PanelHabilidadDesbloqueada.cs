using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel que muestra el nombre, ícono y descripción de una habilidad recién
/// desbloqueada. Congela el juego y libera el cursor mientras está visible
/// (igual que el menú de pausa y Game Over), y los restaura al cerrarlo.
/// </summary>
public class PanelHabilidadDesbloqueada : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private Image imagenIcono;
    [SerializeField] private TMP_Text textoDescripcion;

    [Header("Sonidos")]
    [Tooltip("Jingle al mostrar la habilidad nueva.")]
    [SerializeField] private SonidoAleatorio sonidoMostrar = new SonidoAleatorio();

    public void Mostrar(Habilidad habilidad)
    {
        if (textoNombre != null) textoNombre.text = habilidad.nombreHabilidad;
        if (textoDescripcion != null) textoDescripcion.text = habilidad.descripcion;

        if (imagenIcono != null)
        {
            imagenIcono.sprite = habilidad.icono;
            imagenIcono.enabled = habilidad.icono != null;
        }

        if (panel != null) panel.SetActive(true);
        ReproductorAudio.Reproducir2D(sonidoMostrar);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Cerrar()
    {
        if (panel != null) panel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}