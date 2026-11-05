using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animación de humo estilo caricatura: varias nubecitas (esferas) que salen del centro,
/// se inflan rápido, suben un poco y se encogen hasta desaparecer. Al terminar se destruye sola.
/// No se agrega a mano: la crea <see cref="EfectoMuerteEnemigo"/> con <see cref="Crear"/>.
/// El material se copia del enemigo (mismo shader), así funciona igual en el build (WebGL).
/// </summary>
public class AnimacionHumo : MonoBehaviour
{
    private class Nube
    {
        public Transform transform;
        public Vector3 velocidad;
        public float escalaMaxima;
        public float retraso;
    }

    private static Mesh mallaEsfera;
    private static Material materialPorDefecto;

    private static readonly int IdBaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int IdColor = Shader.PropertyToID("_Color");
    private static readonly int IdBaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int IdMainTex = Shader.PropertyToID("_MainTex");

    private readonly List<Nube> nubes = new List<Nube>();
    private Material material;
    private float duracion;
    private float subida;
    private float tiempo;

    /// <summary>Crea un humo nuevo en el mundo y lo empieza a animar.</summary>
    public static AnimacionHumo Crear(Vector3 centro, float radio, int cantidad, float duracion, Color color, Material materialBase)
    {
        GameObject go = new GameObject("HumoMuerte");
        go.transform.position = centro;
        AnimacionHumo humo = go.AddComponent<AnimacionHumo>();
        humo.Iniciar(radio, cantidad, duracion, color, materialBase);
        return humo;
    }

    private void Iniciar(float radio, int cantidad, float duracionTotal, Color color, Material materialBase)
    {
        duracion = duracionTotal;
        subida = radio * 0.8f;
        material = CrearMaterial(materialBase, color);
        Mesh malla = ObtenerMallaEsfera();

        for (int i = 0; i < cantidad; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            dir.y = Mathf.Abs(dir.y) * 0.6f + 0.15f;   // tiende a salir hacia arriba y a los lados
            dir.Normalize();

            GameObject go = new GameObject("Nube");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = dir * radio * 0.3f;
            go.transform.localScale = Vector3.zero;

            go.AddComponent<MeshFilter>().sharedMesh = malla;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            nubes.Add(new Nube
            {
                transform = go.transform,
                velocidad = dir * radio * Random.Range(1.2f, 2.0f),
                escalaMaxima = radio * Random.Range(0.55f, 0.9f),
                retraso = Random.Range(0f, 0.12f)
            });
        }
    }

    private void Update()
    {
        tiempo += Time.deltaTime;
        bool quedaAlguna = false;

        foreach (Nube n in nubes)
        {
            float t = (tiempo - n.retraso) / duracion;
            if (t < 0f) { quedaAlguna = true; continue; }
            if (t >= 1f) { n.transform.localScale = Vector3.zero; continue; }
            quedaAlguna = true;

            // Se infla rápido (30 %) y luego se encoge suavemente.
            float s = t < 0.3f
                ? Mathf.SmoothStep(0f, 1f, t / 0.3f)
                : Mathf.SmoothStep(1f, 0f, (t - 0.3f) / 0.7f);
            n.transform.localScale = Vector3.one * (n.escalaMaxima * s);
            n.transform.localPosition += (n.velocidad * (1f - t) + Vector3.up * subida) * Time.deltaTime;
        }

        if (!quedaAlguna) Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (material != null) Destroy(material);
    }

    private static Material CrearMaterial(Material materialBase, Color color)
    {
        Material origen = materialBase != null ? materialBase : ObtenerMaterialPorDefecto();
        Material m = new Material(origen) { name = "Humo" };
        if (m.HasProperty(IdBaseMap)) m.SetTexture(IdBaseMap, null);
        if (m.HasProperty(IdMainTex)) m.SetTexture(IdMainTex, null);
        if (m.HasProperty(IdBaseColor)) m.SetColor(IdBaseColor, color);
        if (m.HasProperty(IdColor)) m.SetColor(IdColor, color);
        return m;
    }

    private static Mesh ObtenerMallaEsfera()
    {
        if (mallaEsfera == null) CargarPrimitiva();
        return mallaEsfera;
    }

    private static Material ObtenerMaterialPorDefecto()
    {
        if (materialPorDefecto == null) CargarPrimitiva();
        return materialPorDefecto;
    }

    /// <summary>Toma la malla y el material de una esfera primitiva de Unity (disponibles en cualquier build).</summary>
    private static void CargarPrimitiva()
    {
        GameObject temporal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mallaEsfera = temporal.GetComponent<MeshFilter>().sharedMesh;
        materialPorDefecto = temporal.GetComponent<MeshRenderer>().sharedMaterial;
        Destroy(temporal);
    }
}
