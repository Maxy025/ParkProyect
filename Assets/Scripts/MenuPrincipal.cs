using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private string nombreEscenaHub = "Hub";

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

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
