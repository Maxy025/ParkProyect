using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private string nombreEscenaHub = "Hub";

    [SerializeField] private GameObject panelAjustes;

    [Tooltip("Botón 'Salir'. En la versión web se oculta, porque el navegador no permite cerrar la pestaña desde el juego.")]
    [SerializeField] private GameObject botonSalir;

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (botonSalir != null) botonSalir.SetActive(false);
#endif
    }

    public void IrAInicio()
    {
        // Partida nueva: se borran los objetivos, niveles completados y
        // objetos limpiados de la partida anterior.
        ProgresoJuego.Reiniciar();
        SceneManager.LoadScene(nombreEscenaHub);
    }

    public void AbrirAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(true);
    }

    public void Salir()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
