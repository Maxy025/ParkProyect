using UnityEngine;
using UnityEngine.InputSystem;

public class CombateJugador : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private float danoAtaque = 20f;
    [SerializeField] private float rangoAtaque = 1.5f;
    [SerializeField] private float radioAtaque = 0.6f;
    [SerializeField] private float enfriamientoAtaque = 0.5f;
    [SerializeField] private LayerMask mascaraCapaEnemigo = ~0;
    [SerializeField] private string disparadorAnimAtaque = "Attack";

    [Header("Efecto visual (temporal, para pruebas)")]
    [SerializeField] private GameObject efectoAtaquePrefab;
    [SerializeField] private float duracionEfecto = 0.15f;
    [SerializeField] private Color colorEsferaPrueba = new Color(1f, 0.25f, 0.1f);

    private float ultimoTiempoAtaque = -Mathf.Infinity;

    // Animaciones
    private Animator animator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        // Nos aseguramos de que la capa de golpe empiece con peso 1 
        // (el peso lo filtra la máscara, así que no afectará al Idle si la máscara está bien hecha)
        if (animator != null)
        {
            animator.SetLayerWeight(1, 1f);
        }
    }

    public void enAtaqueBase(InputAction.CallbackContext contexto)
    {
        if (!contexto.performed) return;
        if (Time.time < ultimoTiempoAtaque + enfriamientoAtaque) return;

        RealizarAtaque();
    }

    private void RealizarAtaque()
    {
        ultimoTiempoAtaque = Time.time;

        animator.SetTrigger("Golpe");

        // Lógica de daño
        Vector3 origen = transform.position + Vector3.up * 0.5f + transform.forward * (rangoAtaque * 0.5f);
        Collider[] impactos = Physics.OverlapSphere(origen, radioAtaque, mascaraCapaEnemigo);

        foreach (Collider impacto in impactos)
        {
            if (impacto.TryGetComponent(out IDanable danable))
            {
                danable.RecibirDano(danoAtaque);
            }
        }

        MostrarEfectoAtaque(origen);
    }

    private void MostrarEfectoAtaque(Vector3 origen)
    {
        GameObject efecto;

        if (efectoAtaquePrefab != null)
        {
            efecto = Instantiate(efectoAtaquePrefab, origen, Quaternion.identity);
        }
        else
        {
            efecto = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            efecto.name = "EfectoAtaquePrueba";
            efecto.transform.position = origen;
            efecto.transform.localScale = Vector3.one * (radioAtaque * 2f);

            Collider colisionadorEfecto = efecto.GetComponent<Collider>();
            if (colisionadorEfecto != null) Destroy(colisionadorEfecto);

            Renderer renderizadorEfecto = efecto.GetComponent<Renderer>();
            if (renderizadorEfecto != null) renderizadorEfecto.material.color = colorEsferaPrueba;
        }

        Destroy(efecto, duracionEfecto);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origen = transform.position + Vector3.up * 0.5f + transform.forward * (rangoAtaque * 0.5f);
        Gizmos.DrawWireSphere(origen, radioAtaque);
    }
}