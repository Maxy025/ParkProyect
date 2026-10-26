using UnityEngine;

/// <summary>
/// Habilidad 1: Cortar / podar maleza (se desbloquea al completar el nivel 1).
/// Al equiparla, el jugador sostiene unas tijeras. Cualquier objeto marcado
/// como "Maleza" (ver Maleza.cs) que toque el collider de las tijeras se corta
/// automáticamente (ver CortadorMaleza.cs, que debe ir dentro de modeloTijeras).
/// </summary>
public class HabilidadCortarMaleza : Habilidad
{
    [Tooltip("GameObject de las tijeras en la mano del jugador. Debe tener un " +
             "Collider (Is Trigger) con el componente CortadorMaleza. Se activa/desactiva al equipar.")]
    [SerializeField] private GameObject modeloTijeras;
    
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
        if (modeloTijeras != null) modeloTijeras.SetActive(true);
        animator.SetBool("tieneTijeras", true);
    }

    public override void AlDesequipar()
    {
        if (modeloTijeras != null) modeloTijeras.SetActive(false);
        animator.SetBool("tieneTijeras", false);
    }
}
