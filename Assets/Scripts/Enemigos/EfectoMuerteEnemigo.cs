using UnityEngine;

/// <summary>
/// Genera una nube de humo estilo caricatura cuando el enemigo muere.
/// Se suscribe a <see cref="VidaEnemigo.AlMorir"/> y crea, en el lugar del enemigo,
/// un objeto independiente con <see cref="AnimacionHumo"/> (así el humo sigue aunque
/// el enemigo se destruya en ese mismo frame). El tamaño se ajusta solo al modelo.
/// </summary>
[RequireComponent(typeof(VidaEnemigo))]
public class EfectoMuerteEnemigo : MonoBehaviour
{
    [Header("Humo")]
    [Tooltip("Color de las nubes de humo.")]
    [SerializeField] private Color colorHumo = new Color(0.88f, 0.88f, 0.85f, 1f);

    [Tooltip("Cuántas nubecitas forman el humo.")]
    [SerializeField, Min(1)] private int cantidadNubes = 10;

    [Tooltip("Segundos que dura el humo antes de desaparecer.")]
    [SerializeField, Min(0.1f)] private float duracion = 0.8f;

    [Tooltip("Multiplicador del tamaño del humo (1 = proporcional al enemigo).")]
    [SerializeField, Min(0.1f)] private float multiplicadorTamano = 1f;

    [Tooltip("Materiales que se ignoran al copiar el estilo del enemigo (por ejemplo, el contorno).")]
    [SerializeField] private string ignorarMaterialesCon = "Contorno";

    private VidaEnemigo vida;

    private void Awake()
    {
        vida = GetComponent<VidaEnemigo>();
    }

    private void Start()
    {
        vida.AlMorir.AddListener(GenerarHumo);
    }

    private void OnDestroy()
    {
        if (vida != null) vida.AlMorir.RemoveListener(GenerarHumo);
    }

    /// <summary>Crea el humo en la posición y al tamaño actual del enemigo.</summary>
    public void GenerarHumo()
    {
        Bounds limites = new Bounds(transform.position, Vector3.zero);
        bool hayLimites = false;
        Material materialBase = null;

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r is ParticleSystemRenderer) continue;

            if (!hayLimites) { limites = r.bounds; hayLimites = true; }
            else limites.Encapsulate(r.bounds);

            if (materialBase == null)
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    if (!string.IsNullOrEmpty(ignorarMaterialesCon) && m.name.Contains(ignorarMaterialesCon)) continue;
                    materialBase = m;
                    break;
                }
            }
        }

        Vector3 centro = hayLimites ? limites.center : transform.position + Vector3.up * 0.5f;
        float radio = hayLimites ? Mathf.Clamp(limites.extents.magnitude * 0.5f, 0.3f, 2.5f) : 0.6f;

        AnimacionHumo.Crear(centro, radio * multiplicadorTamano, cantidadNubes, duracion, colorHumo, materialBase);
    }
}
