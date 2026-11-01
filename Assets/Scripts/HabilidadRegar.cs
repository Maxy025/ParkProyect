using UnityEngine;

/// <summary>
/// Habilidad 2: Regar / esparcir agua (se desbloquea al completar el nivel 2).
/// Al mantener presionado el botón de habilidad, una esfera de agua crece
/// sobre el jugador. Al soltar, explota esparciendo agua en un radio
/// proporcional al tiempo que se mantuvo presionada la tecla.
/// </summary>
public class HabilidadRegar : Habilidad
{
    [Header("Visual de la esfera")]
    [Tooltip("Objeto hijo del jugador que representa la esfera de agua (oculto por defecto)")]
    [SerializeField] private GameObject visualEsferaAgua;
    [SerializeField] private Transform puntoAparicionEsferaAgua;

    [Header("Carga (mantener presionado)")]
    [SerializeField] private float tiempoCargaMaxima = 2.5f;
    [SerializeField] private float escalaMinima = 0.2f;
    [SerializeField] private float escalaMaxima = 1.5f;

    [Header("Explosión / esparcido")]
    [SerializeField] private float radioSalpicaduraMinimo = 1.5f;
    [SerializeField] private float radioSalpicaduraMaximo = 6f;
    [SerializeField] private GameObject prefabEfectoSalpicadura;
    [SerializeField] private LayerMask mascaraCapaSalpicadura = ~0;

    [Header("Sonidos")]
    [Tooltip("Sonido en bucle mientras se carga la esfera. Su tono sube conforme crece.")]
    [SerializeField] private AudioClip sonidoCarga;
    [SerializeField, Range(0f, 1f)] private float volumenCarga = 0.6f;
    [SerializeField] private float tonoCargaInicial = 0.8f;
    [SerializeField] private float tonoCargaFinal = 1.4f;
    [Tooltip("Sonido de la explosión de agua al soltar.")]
    [SerializeField] private SonidoAleatorio sonidoSalpicadura = new SonidoAleatorio();

    private AudioSource fuenteCarga;
    private float tiempoCarga = 0f;
    private bool estaCargando = false;

    //Animaciones
    private Animator animator;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetLayerWeight(1, 1f);
        }
    }

    public override void AlEquipar()
    {
        if (visualEsferaAgua != null) visualEsferaAgua.SetActive(false);
        animator.SetBool("tieneSpray", true);
    }

    public override void AlDesequipar()
    {
        animator.SetBool("tieneSpray", false);
        CancelarCarga();
    }

    public override void AlPresionarActivar()
    {
        estaCargando = true;
        tiempoCarga = 0f;

        if (visualEsferaAgua != null)
        {
            visualEsferaAgua.SetActive(true);
            visualEsferaAgua.transform.localScale = Vector3.one * escalaMinima;
        }

        if (sonidoCarga != null)
        {
            if (fuenteCarga == null) fuenteCarga = ReproductorAudio.CrearFuenteBucle(gameObject, 0f);
            fuenteCarga.clip = sonidoCarga;
            fuenteCarga.pitch = tonoCargaInicial;
            fuenteCarga.volume = volumenCarga * ReproductorAudio.FactorEfectos;
            fuenteCarga.Play();
        }
    }

    public override void AlMantenerActivar()
    {
        if (!estaCargando) return;

        tiempoCarga = Mathf.Min(tiempoCarga + Time.deltaTime, tiempoCargaMaxima);

        float t = tiempoCarga / tiempoCargaMaxima;

        if (visualEsferaAgua != null)
            visualEsferaAgua.transform.localScale = Vector3.one * Mathf.Lerp(escalaMinima, escalaMaxima, t);

        if (fuenteCarga != null && fuenteCarga.isPlaying)
            fuenteCarga.pitch = Mathf.Lerp(tonoCargaInicial, tonoCargaFinal, t);
    }

    public override void AlSoltarActivar()
    {
        if (!estaCargando) return;
        Explotar();
    }

    private void Explotar()
    {
        float t = tiempoCarga / tiempoCargaMaxima;
        float radio = Mathf.Lerp(radioSalpicaduraMinimo, radioSalpicaduraMaximo, t);
        Vector3 origen = puntoAparicionEsferaAgua != null
            ? puntoAparicionEsferaAgua.position
            : jugador.position + Vector3.up * 1.8f;

        if (prefabEfectoSalpicadura != null)
        {
            GameObject efecto = Instantiate(prefabEfectoSalpicadura, origen, Quaternion.identity);
            efecto.transform.localScale = Vector3.one * Mathf.Max(0.1f, radio / radioSalpicaduraMaximo);
        }

        ReproductorAudio.ReproducirEn(sonidoSalpicadura, origen, 0.5f);

        Collider[] impactos = Physics.OverlapSphere(origen, radio, mascaraCapaSalpicadura);
        foreach (Collider impacto in impactos)
        {
            if (impacto.TryGetComponent(out ZonaPeligrosa peligro))
                peligro.LimpiarPeligro();

            if (impacto.TryGetComponent(out Regable planta))
                planta.Regar(t);
        }

        CancelarCarga();
    }

    private void CancelarCarga()
    {
        estaCargando = false;
        tiempoCarga = 0f;
        if (visualEsferaAgua != null) visualEsferaAgua.SetActive(false);
        if (fuenteCarga != null) fuenteCarga.Stop();
    }

    private void OnDrawGizmosSelected()
    {
        if (!estaCargando || jugador == null) return;
        float t = tiempoCarga / tiempoCargaMaxima;
        float radio = Mathf.Lerp(radioSalpicaduraMinimo, radioSalpicaduraMaximo, t);
        Vector3 origen = puntoAparicionEsferaAgua != null ? puntoAparicionEsferaAgua.position : jugador.position + Vector3.up * 1.8f;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origen, radio);
    }
}
