using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manager persistente (DontDestroyOnLoad) que centraliza la navegación
/// entre escenas: carga niveles, reubica al jugador en el PuntoAparicion
/// correcto tras cargar, y opcionalmente reinicia su vida.
/// Colócalo junto a GestorAudio en la primera escena de tu juego.
/// </summary>
public class GestorNiveles : MonoBehaviour
{
    private static GestorNiveles instancia;

    private string idPuntoAparicionPendiente = "";
    private bool restablecerVidaPendiente = false;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDestroy()
    {
        if (instancia == this)
            SceneManager.sceneLoaded -= AlCargarEscena;
    }

    /// <summary>
    /// Carga una escena y, una vez cargada, mueve al jugador al PuntoAparicion
    /// cuyo Id coincida con idPuntoAparicion. Si restablecerVida es true,
    /// además cura al jugador al máximo (útil para reinicios de nivel).
    /// </summary>
    public static void CargarNivel(string nombreEscena, string idPuntoAparicion, bool restablecerVida = false)
    {
        if (instancia == null)
        {
            Debug.LogWarning("GestorNiveles: no hay instancia en la escena. ¿Olvidaste colocarlo?");
            SceneManager.LoadScene(nombreEscena);
            return;
        }

        instancia.idPuntoAparicionPendiente = idPuntoAparicion;
        instancia.restablecerVidaPendiente = restablecerVida;
        SceneManager.LoadScene(nombreEscena);
    }

    /// <summary>
    /// Destruye al jugador persistente y carga el menú principal, para que
    /// la próxima partida empiece limpia en vez de arrastrar vida/estado.
    /// </summary>
    public static void SalirAMenuPrincipal(string nombreEscenaMenu)
    {
        JugadorPersistente.DestruirInstancia();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(nombreEscenaMenu);
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        JugadorPersistente jugador = JugadorPersistente.Obtener();
        if (jugador == null) return;

        if (!string.IsNullOrEmpty(idPuntoAparicionPendiente))
        {
            PuntoAparicion[] puntos = FindObjectsOfType<PuntoAparicion>();
            bool encontrado = false;

            foreach (PuntoAparicion punto in puntos)
            {
                if (punto.Id == idPuntoAparicionPendiente)
                {
                    jugador.Reubicar(punto.transform.position, punto.transform.rotation);
                    encontrado = true;
                    break;
                }
            }

            if (!encontrado)
            {
                Debug.LogWarning($"GestorNiveles: no se encontró ningún PuntoAparicion con Id " +
                    $"'{idPuntoAparicionPendiente}' en la escena '{escena.name}'. " +
                    $"El jugador se quedó donde estaba. Revisa que el Id coincida exactamente " +
                    $"(mayúsculas incluidas) con algún PuntoAparicion de esta escena.");
            }
        }

        if (restablecerVidaPendiente && jugador.VidaJugador != null)
        {
            jugador.VidaJugador.ResetHealth();
        }

        idPuntoAparicionPendiente = "";
        restablecerVidaPendiente = false;
    }
}