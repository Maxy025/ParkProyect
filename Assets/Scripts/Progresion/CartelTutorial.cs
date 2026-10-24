using UnityEngine;

/// <summary>
/// Cartel del tutorial con una flecha flotante encima. Cuando el jugador está cerca,
/// aparece el aviso "Presiona F para leer" y, al presionar el botón de interactuar,
/// PanelTutorial muestra el título, el mensaje y el objetivo de este cartel.
///
/// Necesita un Collider con Is Trigger que marque la zona desde donde se puede leer,
/// y un PanelTutorial en el Canvas de la escena.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CartelTutorial : MonoBehaviour
{
    [Header("Contenido")]
    [SerializeField] private string titulo = "Ataque";

    [TextArea(3, 8)]
    [Tooltip("Explicación de la mecánica (controles, consejos...).")]
    [SerializeField] private string mensaje = "Presiona clic izquierdo para atacar a los enemigos.";

    [TextArea(2, 4)]
    [Tooltip("Objetivo actual (opcional). Si se deja vacío, no se muestra.")]
    [SerializeField] private string objetivo = "";

    [Header("Flecha indicadora")]
    [Tooltip("Objeto hijo con la flecha (modelo 3D o sprite) que flota sobre el cartel.")]
    [SerializeField] private Transform flecha;

    [Tooltip("Cuánto sube y baja la flecha.")]
    [SerializeField] private float alturaRebote = 0.25f;

    [Tooltip("Qué tan rápido sube y baja la flecha.")]
    [SerializeField] private float velocidadRebote = 2f;

    [Tooltip("Grados por segundo que gira la flecha sobre su eje vertical (0 = no gira).")]
    [SerializeField] private float velocidadGiro = 90f;

    [Tooltip("Oculta la flecha después de que el jugador lee el cartel, para saber cuáles faltan.")]
    [SerializeField] private bool ocultarFlechaAlLeer = false;

    private Vector3 posicionInicialFlecha;
    private bool leido = false;

    public string Titulo => titulo;
    public string Mensaje => mensaje;
    public string Objetivo => objetivo;
    public bool Leido => leido;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        if (flecha != null) posicionInicialFlecha = flecha.localPosition;
    }

    private void Update()
    {
        if (flecha == null || !flecha.gameObject.activeSelf) return;

        float desplazamiento = Mathf.Sin(Time.time * velocidadRebote) * alturaRebote;
        flecha.localPosition = posicionInicialFlecha + Vector3.up * desplazamiento;

        if (velocidadGiro != 0f)
            flecha.Rotate(Vector3.up, velocidadGiro * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (PanelTutorial.Instancia != null)
            PanelTutorial.Instancia.RegistrarCartelCercano(this);
        else
            Debug.LogWarning("[CartelTutorial] No hay un PanelTutorial en la escena.", this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (PanelTutorial.Instancia != null)
            PanelTutorial.Instancia.QuitarCartelCercano(this);
    }

    private void OnDisable()
    {
        if (PanelTutorial.Instancia != null)
            PanelTutorial.Instancia.QuitarCartelCercano(this);
    }

    /// <summary>Lo llama PanelTutorial al mostrar este cartel.</summary>
    public void MarcarLeido()
    {
        leido = true;
        if (ocultarFlechaAlLeer && flecha != null)
            flecha.gameObject.SetActive(false);
    }
}
