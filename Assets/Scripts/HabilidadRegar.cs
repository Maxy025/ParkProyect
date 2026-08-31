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

    private float tiempoCarga = 0f;
    private bool estaCargando = false;

    public override void AlEquipar()
    {
        if (visualEsferaAgua != null) visualEsferaAgua.SetActive(false);
    }

    public override void AlDesequipar()
    {
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
    }

    public override void AlMantenerActivar()
    {
        if (!estaCargando) return;

        tiempoCarga = Mathf.Min(tiempoCarga + Time.deltaTime, tiempoCargaMaxima);

        if (visualEsferaAgua != null)
        {
            float t = tiempoCarga / tiempoCargaMaxima;
            visualEsferaAgua.transform.localScale = Vector3.one * Mathf.Lerp(escalaMinima, escalaMaxima, t);
        }
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
