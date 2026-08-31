using UnityEngine;

/// <summary>
/// Habilidad 3: Recolección de basura (se desbloquea al iniciar el nivel 3).
/// Al equipar, aparece un bote de basura cerca del jugador. El jugador
/// recolecta basura del entorno al tocarla (hasta un límite) y debe
/// llevarla al bote para eliminarla. Cargar más basura reduce su velocidad.
/// </summary>
public class HabilidadRecolectarBasura : Habilidad
{
    [Header("Bote de basura")]
    [SerializeField] private GameObject prefabBoteBasura;
    [SerializeField] private Vector3 desplazamientoAparicion = new Vector3(1.5f, 0f, 1.5f);
    [Tooltip("Si está activo, ajusta la altura del bote buscando el suelo real con un raycast, " +
             "en vez de usar el offset de Y tal cual. Solo actívalo si tu pivote de jugador no " +
             "está a nivel del suelo y el bote aparece flotando o enterrado con el offset simple.")]
    [SerializeField] private bool ajustarAlSueloConRaycast = false;
    [SerializeField] private float distanciaBusquedaSuelo = 5f;
    [SerializeField] private LayerMask capaSuelo = ~0;

    [Header("Inventario")]
    [SerializeField] private int capacidadCargaMaxima = 5;
    [Tooltip("Multiplicador de velocidad al llevar la carga máxima (1 = sin penalización)")]
    [SerializeField, Range(0.1f, 1f)] private float multiplicadorVelocidadCargaMaxima = 0.5f;

    private GameObject boteBasuraGenerado;
    private int basuraCargada = 0;
    private bool estaEquipada = false;

    public int BasuraCargada => basuraCargada;
    public int CapacidadCargaMaxima => capacidadCargaMaxima;

    public override void AlEquipar()
    {
        estaEquipada = true;

        if (prefabBoteBasura != null && boteBasuraGenerado == null)
        {
            Vector3 posicionAparicion = jugador.position + jugador.TransformDirection(desplazamientoAparicion);

            if (ajustarAlSueloConRaycast)
            {
  
                Vector3 origenRayo = posicionAparicion + Vector3.up * distanciaBusquedaSuelo;
                if (Physics.Raycast(origenRayo, Vector3.down, out RaycastHit impacto,
                    distanciaBusquedaSuelo * 2f, capaSuelo, QueryTriggerInteraction.Ignore))
                {
                    posicionAparicion.y = impacto.point.y;
                }
            }

            boteBasuraGenerado = Instantiate(prefabBoteBasura, posicionAparicion, Quaternion.identity);

            if (boteBasuraGenerado.TryGetComponent(out BoteBasura bote))
                bote.Inicializar(this);
        }
    }

    public override void AlDesequipar()
    {
        estaEquipada = false;

        if (boteBasuraGenerado != null)
        {
            Destroy(boteBasuraGenerado);
            boteBasuraGenerado = null;
        }
    }

    /// <summary>Llamado por ObjetoBasura cuando el jugador toca basura en el mundo.</summary>
    public bool IntentarRecolectar()
    {
        if (!estaEquipada) return false;
        if (basuraCargada >= capacidadCargaMaxima) return false;

        basuraCargada++;
        ActualizarVelocidadMovimiento();
        return true;
    }

    /// <summary>Llamado por BoteBasura cuando el jugador deposita su carga.</summary>
    public int DepositarTodo()
    {
        int depositado = basuraCargada;
        basuraCargada = 0;
        ActualizarVelocidadMovimiento();
        return depositado;
    }

    private void ActualizarVelocidadMovimiento()
    {
        float proporcionCarga = capacidadCargaMaxima > 0 ? (float)basuraCargada / capacidadCargaMaxima : 0f;
        float multiplicador = Mathf.Lerp(1f, multiplicadorVelocidadCargaMaxima, proporcionCarga);

        if (jugador != null && jugador.TryGetComponent(out IVelocidadModificable modificable))
        {
            modificable.EstablecerMultiplicadorVelocidad(multiplicador);
        }
    }
}