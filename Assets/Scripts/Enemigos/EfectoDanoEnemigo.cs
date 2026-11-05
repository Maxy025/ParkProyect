using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hace parpadear en rojo al enemigo cada vez que recibe daño (sin morir).
/// Se suscribe a <see cref="VidaEnemigo.AlRecibirDano"/> y, durante el destello,
/// cambia los materiales de todos sus Renderer hijos por una copia roja sin textura.
/// Los materiales de contorno (por nombre) se dejan intactos para conservar la línea negra.
/// Funciona con cualquier modelo hijo (slime, murciélago, cápsula) sin configurar nada.
/// </summary>
[RequireComponent(typeof(VidaEnemigo))]
public class EfectoDanoEnemigo : MonoBehaviour
{
    [Header("Destello")]
    [Tooltip("Color con el que parpadea el enemigo al recibir daño.")]
    [SerializeField] private Color colorDestello = new Color(1f, 0.15f, 0.15f, 1f);

    [Tooltip("Cuántas veces parpadea por cada golpe.")]
    [SerializeField, Min(1)] private int parpadeos = 2;

    [Tooltip("Segundos que dura cada destello encendido (y el mismo tiempo apagado).")]
    [SerializeField, Min(0.01f)] private float duracionDestello = 0.08f;

    [Tooltip("Los materiales cuyo nombre contenga este texto no se pintan de rojo (por ejemplo, el contorno negro). Déjalo vacío para pintar todo.")]
    [SerializeField] private string ignorarMaterialesCon = "Contorno";

    private VidaEnemigo vida;
    private Coroutine rutinaParpadeo;
    private Renderer[] renderers;
    private Material[][] materialesOriginales;

    // Una copia roja por material original, para no crear materiales en cada golpe.
    private readonly Dictionary<Material, Material> copiasRojas = new Dictionary<Material, Material>();

    private static readonly int IdBaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int IdColor = Shader.PropertyToID("_Color");
    private static readonly int IdBaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int IdMainTex = Shader.PropertyToID("_MainTex");

    private void Awake()
    {
        vida = GetComponent<VidaEnemigo>();
    }

    private void Start()
    {
        vida.AlRecibirDano.AddListener(Parpadear);
    }

    private void OnDestroy()
    {
        if (vida != null) vida.AlRecibirDano.RemoveListener(Parpadear);

        foreach (Material copia in copiasRojas.Values)
            if (copia != null) Destroy(copia);
        copiasRojas.Clear();
    }

    private void OnDisable()
    {
        // Si el enemigo se desactiva a media animación, regresa sus materiales.
        if (rutinaParpadeo != null)
        {
            StopCoroutine(rutinaParpadeo);
            rutinaParpadeo = null;
        }
        RestaurarMateriales();
    }

    /// <summary>Inicia el parpadeo rojo. Si ya estaba parpadeando, lo reinicia.</summary>
    public void Parpadear()
    {
        if (!isActiveAndEnabled) return;

        if (rutinaParpadeo != null)
        {
            StopCoroutine(rutinaParpadeo);
            RestaurarMateriales();
        }
        rutinaParpadeo = StartCoroutine(RutinaParpadeo());
    }

    private IEnumerator RutinaParpadeo()
    {
        GuardarMaterialesActuales();
        WaitForSeconds espera = new WaitForSeconds(duracionDestello);

        for (int i = 0; i < parpadeos; i++)
        {
            AplicarRojo();
            yield return espera;
            RestaurarMateriales();
            yield return espera;
        }

        rutinaParpadeo = null;
    }

    /// <summary>Lee los materiales justo antes de parpadear (por si otro script los cambió en Start).</summary>
    private void GuardarMaterialesActuales()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        materialesOriginales = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
            materialesOriginales[i] = renderers[i].sharedMaterials;
    }

    private void AplicarRojo()
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material[] originales = materialesOriginales[i];
            Material[] rojos = new Material[originales.Length];
            for (int j = 0; j < originales.Length; j++)
                rojos[j] = ObtenerCopiaRoja(originales[j]);

            renderers[i].sharedMaterials = rojos;
        }
    }

    private void RestaurarMateriales()
    {
        if (renderers == null || materialesOriginales == null) return;

        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null)
                renderers[i].sharedMaterials = materialesOriginales[i];
    }

    /// <summary>
    /// Crea (una sola vez) una copia del material con el color del destello y sin textura,
    /// usando el mismo shader del original para que funcione igual en el build (WebGL).
    /// </summary>
    private Material ObtenerCopiaRoja(Material original)
    {
        if (original == null) return null;
        if (!string.IsNullOrEmpty(ignorarMaterialesCon) && original.name.Contains(ignorarMaterialesCon))
            return original;

        if (copiasRojas.TryGetValue(original, out Material copia) && copia != null)
            return copia;

        copia = new Material(original) { name = original.name + "_Destello" };
        if (copia.HasProperty(IdBaseMap)) copia.SetTexture(IdBaseMap, null);
        if (copia.HasProperty(IdMainTex)) copia.SetTexture(IdMainTex, null);
        if (copia.HasProperty(IdBaseColor)) copia.SetColor(IdBaseColor, colorDestello);
        if (copia.HasProperty(IdColor)) copia.SetColor(IdColor, colorDestello);

        copiasRojas[original] = copia;
        return copia;
    }
}
