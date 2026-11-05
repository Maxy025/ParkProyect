using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Efecto visual cuando el jugador recibe daño, distinto al de los enemigos:
/// primero un destello blanco muy corto y después un parpadeo de visible/invisible
/// (estilo "invulnerabilidad") durante casi un segundo.
/// Se suscribe al evento OnDamageTaken de PlayerHealth sin modificar ese script.
/// Va en el mismo objeto que PlayerHealth (el Player).
/// </summary>
public class EfectoDanoJugador : MonoBehaviour
{
    [Tooltip("Objeto con el modelo del jugador. Si se deja vacío se usan todos los modelos hijos de este objeto.")]
    [SerializeField] private Transform modelo;

    [Header("Destello inicial")]
    [Tooltip("Si se activa, el jugador se pone de un color sólido por un instante al recibir el golpe.")]
    [SerializeField] private bool usarDestello = true;

    [Tooltip("Color del destello inicial (blanco para diferenciarlo del rojo de los enemigos).")]
    [SerializeField] private Color colorDestello = Color.white;

    [Tooltip("Segundos que dura el destello inicial.")]
    [SerializeField, Min(0.01f)] private float duracionDestello = 0.08f;

    [Header("Parpadeo")]
    [Tooltip("Segundos totales que el jugador parpadea después del golpe.")]
    [SerializeField, Min(0f)] private float duracionParpadeo = 0.9f;

    [Tooltip("Segundos entre cada encendido/apagado del modelo.")]
    [SerializeField, Min(0.02f)] private float intervaloParpadeo = 0.07f;

    [Tooltip("Materiales que no se pintan en el destello (por ejemplo, el contorno).")]
    [SerializeField] private string ignorarMaterialesCon = "Contorno";

    private PlayerHealth vida;
    private Coroutine rutina;
    private readonly List<Renderer> renderersActivos = new List<Renderer>();
    private readonly List<Material[]> materialesOriginales = new List<Material[]>();
    private readonly Dictionary<Material, Material> copiasDestello = new Dictionary<Material, Material>();

    private static readonly int IdBaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int IdColor = Shader.PropertyToID("_Color");
    private static readonly int IdBaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int IdMainTex = Shader.PropertyToID("_MainTex");

    private void Awake()
    {
        vida = GetComponent<PlayerHealth>();
        if (vida == null) vida = GetComponentInParent<PlayerHealth>();
    }

    private void Start()
    {
        if (vida != null) vida.OnDamageTaken.AddListener(Parpadear);
        else Debug.LogWarning("EfectoDanoJugador: no se encontró PlayerHealth en este objeto ni en sus padres.", this);
    }

    private void OnDestroy()
    {
        if (vida != null) vida.OnDamageTaken.RemoveListener(Parpadear);
        foreach (Material m in copiasDestello.Values)
            if (m != null) Destroy(m);
        copiasDestello.Clear();
    }

    private void OnDisable()
    {
        Detener();
    }

    /// <summary>Inicia el efecto de daño. Si ya estaba activo, lo reinicia.</summary>
    public void Parpadear()
    {
        if (!isActiveAndEnabled) return;
        Detener();
        rutina = StartCoroutine(RutinaDano());
    }

    private IEnumerator RutinaDano()
    {
        GuardarRenderers();

        if (usarDestello)
        {
            AplicarDestello();
            yield return new WaitForSeconds(duracionDestello);
            RestaurarMateriales();
        }

        WaitForSeconds espera = new WaitForSeconds(intervaloParpadeo);
        float fin = Time.time + duracionParpadeo;
        bool visible = true;

        while (Time.time < fin)
        {
            if (vida != null && vida.IsDead) break;   // al morir se queda visible
            visible = !visible;
            MostrarModelo(visible);
            yield return espera;
        }

        MostrarModelo(true);
        rutina = null;
    }

    private void Detener()
    {
        if (rutina != null)
        {
            StopCoroutine(rutina);
            rutina = null;
        }
        RestaurarMateriales();
        MostrarModelo(true);
    }

    /// <summary>Guarda solo los modelos que estaban visibles, para no encender cosas ocultas (como herramientas guardadas).</summary>
    private void GuardarRenderers()
    {
        renderersActivos.Clear();
        materialesOriginales.Clear();
        Transform raiz = modelo != null ? modelo : transform;

        foreach (Renderer r in raiz.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled) continue;
            if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
            renderersActivos.Add(r);
            materialesOriginales.Add(r.sharedMaterials);
        }
    }

    private void MostrarModelo(bool visible)
    {
        foreach (Renderer r in renderersActivos)
            if (r != null) r.enabled = visible;
    }

    private void AplicarDestello()
    {
        for (int i = 0; i < renderersActivos.Count; i++)
        {
            Renderer r = renderersActivos[i];
            if (r == null) continue;
            Material[] originales = materialesOriginales[i];
            Material[] destello = new Material[originales.Length];
            for (int j = 0; j < originales.Length; j++)
                destello[j] = ObtenerCopiaDestello(originales[j]);
            r.sharedMaterials = destello;
        }
    }

    private void RestaurarMateriales()
    {
        for (int i = 0; i < renderersActivos.Count && i < materialesOriginales.Count; i++)
            if (renderersActivos[i] != null)
                renderersActivos[i].sharedMaterials = materialesOriginales[i];
    }

    private Material ObtenerCopiaDestello(Material original)
    {
        if (original == null) return null;
        if (!string.IsNullOrEmpty(ignorarMaterialesCon) && original.name.Contains(ignorarMaterialesCon))
            return original;
        if (copiasDestello.TryGetValue(original, out Material copia) && copia != null)
            return copia;

        copia = new Material(original) { name = original.name + "_DestelloJugador" };
        if (copia.HasProperty(IdBaseMap)) copia.SetTexture(IdBaseMap, null);
        if (copia.HasProperty(IdMainTex)) copia.SetTexture(IdMainTex, null);
        if (copia.HasProperty(IdBaseColor)) copia.SetColor(IdBaseColor, colorDestello);
        if (copia.HasProperty(IdColor)) copia.SetColor(IdColor, colorDestello);

        copiasDestello[original] = copia;
        return copia;
    }
}
