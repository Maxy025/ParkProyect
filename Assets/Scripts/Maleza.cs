using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Marca un objeto como "hierba mala" cortable (césped alto, hiedra,
/// pastizal, arbustos, etc.). Colócalo en cualquier prefab de vegetación
/// que deba desaparecer al ser cortado con la habilidad de tijeras.
/// </summary>
public class Maleza : MonoBehaviour
{
    [Header("Al cortar")]
    [SerializeField] private GameObject prefabEfectoCorte;
    [SerializeField] private AudioClip sonidoCorte;
    [Tooltip("Si está activo, el objeto se destruye. Si no, solo se desactiva.")]
    [SerializeField] private bool destruirAlCortar = true;

    [Tooltip("Úsalo para abrir un camino, revelar un coleccionable, activar una puerta, etc.")]
    public UnityEvent AlCortar;

    private bool yaCortada = false;

    public void Cortar()
    {
        if (yaCortada) return;
        yaCortada = true;

        if (prefabEfectoCorte != null)
            Instantiate(prefabEfectoCorte, transform.position, Quaternion.identity);

        if (sonidoCorte != null)
            AudioSource.PlayClipAtPoint(sonidoCorte, transform.position);

        AlCortar?.Invoke();

        if (destruirAlCortar) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
