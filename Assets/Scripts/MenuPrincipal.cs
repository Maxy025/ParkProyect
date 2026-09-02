using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla los 3 botones del menú de inicio: Inicio, Ajustes y Salir.
/// Engancha estos métodos desde el OnClick() de cada botón en el Inspector.
/// </summary>
public class MenuPrincipal : MonoBehaviour
{
    [Header("Escena de destino")]
    [Tooltip("Nombre EXACTO de la escena del parque hub. Debe estar agregada " +
             "en File > Build Settings > Scenes In Build.")]
    [SerializeField] private string nombreEscenaHub = "Hub";

    [Header("Referencias")]
    [SerializeField] private GameObject panelAjustes;

    public void IrAInicio()
    {
        SceneManager.LoadScene(nombreEscenaHub);
    }

    public void AbrirAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(true);
    }

    public void Salir()
    {
        // Application.Quit() no hace nada dentro del Editor, así que en modo
        // Editor detenemos el Play mode en su lugar para poder probarlo.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
