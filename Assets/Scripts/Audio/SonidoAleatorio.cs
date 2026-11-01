using UnityEngine;

/// <summary>
/// Un sonido con variaciones: una lista de clips de la que se elige uno al azar
/// (sin repetir el anterior) y un rango de tono (pitch) aleatorio.
/// Así los sonidos repetidos (pasos, golpes, cortes) no suenan iguales cada vez.
/// Se usa como campo en los scripts: [SerializeField] private SonidoAleatorio sonidoX;
/// Si la lista de clips está vacía, simplemente no suena nada.
/// </summary>
[System.Serializable]
public class SonidoAleatorio
{
    [Tooltip("Arrastra aquí uno o varios clips. Si hay varios, se elige uno al azar cada vez.")]
    public AudioClip[] clips;

    [Tooltip("Volumen del sonido (0-1).")]
    [Range(0f, 1f)] public float volumen = 1f;

    [Tooltip("Tono mínimo y máximo (1 = normal). Ej. 0.9 - 1.1 para que varíe un poco cada vez.")]
    public Vector2 rangoTono = new Vector2(0.95f, 1.05f);

    [System.NonSerialized] private int ultimoIndice = -1;

    /// <summary>Indica si hay al menos un clip asignado.</summary>
    public bool TieneClips
    {
        get
        {
            if (clips == null) return false;
            foreach (AudioClip clip in clips)
                if (clip != null) return true;
            return false;
        }
    }

    /// <summary>Elige un clip al azar evitando repetir el último (si hay más de uno).</summary>
    public AudioClip Elegir()
    {
        if (!TieneClips) return null;
        if (clips.Length == 1) return clips[0];

        int indice = Random.Range(0, clips.Length);
        if (indice == ultimoIndice) indice = (indice + 1) % clips.Length;

        // Si el elegido está vacío, busca el siguiente que sí tenga clip.
        for (int i = 0; i < clips.Length; i++)
        {
            int candidato = (indice + i) % clips.Length;
            if (clips[candidato] != null)
            {
                ultimoIndice = candidato;
                return clips[candidato];
            }
        }

        return null;
    }

    /// <summary>Tono aleatorio dentro del rango.</summary>
    public float ElegirTono()
    {
        float minimo = Mathf.Max(0.1f, Mathf.Min(rangoTono.x, rangoTono.y));
        float maximo = Mathf.Max(0.1f, Mathf.Max(rangoTono.x, rangoTono.y));
        return Random.Range(minimo, maximo);
    }
}
