using UnityEngine;

public class AplicarToonGlobal : MonoBehaviour
{
    public Material toonMaterial; // Arrastra aquí tu material con el ToonShader

    void Start()
    {
        if (toonMaterial == null) return;

        // Busca todos los MeshRenderer en la escena y cambia sus materiales
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer rend in renderers)
        {
            Material[] mats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = toonMaterial;
            }
            rend.materials = mats;
        }
        Debug.Log("Toon Shader aplicado globalmente a " + renderers.Length + " objetos.");
    }
}