using UnityEngine;

/// <summary>
/// Panel de créditos y agradecimientos (un Canvas sencillo).
/// Congela el juego y libera el cursor mientras está abierto. Opcionalmente
/// desplaza el texto hacia arriba de forma automática, como en los créditos de una película.
/// Se abre con ZonaCreditos o desde cualquier botón llamando a Mostrar().
/// </summary>
public class PanelCreditos : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panel;

    [Header("Desplazamiento automático (opcional)")]
    [Tooltip("RectTransform con todo el texto de los créditos. Déjalo vacío si el texto no se mueve.")]
    [SerializeField] private RectTransform contenido;

    [Tooltip("Pixeles por segundo que sube el texto (0 = sin movimiento).")]
    [SerializeField, Min(0f)] private float velocidadDesplazamiento = 40f;

    [Tooltip("Vuelve a empezar cuando el texto termina de pasar.")]
    [SerializeField] private bool repetir = true;

    [Header("Salida")]
    [SerializeField] private string nombreEscenaMenuPrincipal = "MenuInicio";

    private Vector2 posicionInicial;
    private bool abierto = false;

    public bool EstaAbierto => abierto;

    private void Awake()
    {
        if (contenido != null) posicionInicial = contenido.anchoredPosition;
        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        if (!abierto || contenido == null || velocidadDesplazamiento <= 0f) return;

        // Tiempo sin escalar: el juego está congelado mientras se ven los créditos.
        contenido.anchoredPosition += Vector2.up * velocidadDesplazamiento * Time.unscaledDeltaTime;

        bool termino = contenido.anchoredPosition.y - posicionInicial.y > contenido.rect.height;
        if (termino && repetir)
            contenido.anchoredPosition = posicionInicial;
    }

    public void Mostrar()
    {
        if (abierto) return;

        abierto = true;
        if (contenido != null) contenido.anchoredPosition = posicionInicial;
        if (panel != null) panel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Cierra los créditos y regresa al Hub para seguir explorando. Engánchalo al botón "Continuar".</summary>
    public void Cerrar()
    {
        if (!abierto) return;

        abierto = false;
        if (panel != null) panel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>Engánchalo al botón "Menú principal" (opcional).</summary>
    public void SalirAMenuPrincipal()
    {
        abierto = false;
        Time.timeScale = 1f;
        GestorNiveles.SalirAMenuPrincipal(nombreEscenaMenuPrincipal);
    }
}
