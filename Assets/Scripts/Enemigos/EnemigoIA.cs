using UnityEngine;

/// <summary>
/// Clase base abstracta para la IA de los enemigos.
/// Se encarga de detectar al jugador (radio, cono de visión y línea de visión)
/// y de cambiar entre los estados Patrullando y Persiguiendo.
/// Cada tipo de enemigo (terrestre, volador) define cómo se mueve en cada estado.
/// </summary>
public abstract class EnemigoIA : MonoBehaviour
{
    /// <summary>Estados posibles de la IA.</summary>
    public enum EstadoEnemigo { Patrullando, Persiguiendo }

    [Header("Detección del jugador")]
    [Tooltip("Distancia a la que el enemigo puede ver al jugador mientras patrulla.")]
    [SerializeField] protected float radioDeteccion = 10f;

    [Tooltip("Ángulo total del cono de visión en grados (360 = ve en todas direcciones).")]
    [SerializeField, Range(0f, 360f)] protected float anguloVision = 140f;

    [Tooltip("Distancia a partir de la cual deja de ver al jugador mientras lo persigue. Debe ser mayor que el radio de detección.")]
    [SerializeField] protected float radioPerdida = 16f;

    [Tooltip("Segundos sin ver al jugador antes de rendirse y volver a patrullar.")]
    [SerializeField] protected float tiempoOlvido = 2.5f;

    [Tooltip("Capas que bloquean la vista (terreno, paredes, árboles...). No incluyas la capa de Enemigos.")]
    [SerializeField] protected LayerMask capasObstaculos = ~0;

    [Tooltip("Altura de los 'ojos' del enemigo respecto a su pivote.")]
    [SerializeField] protected float alturaOjos = 1f;

    [Tooltip("Altura del punto del jugador al que mira el enemigo (pecho), medida desde sus pies.")]
    [SerializeField] protected float alturaObjetivo = 1f;

    [Tooltip("Cada cuántos segundos se revisa la visión. Ahorra rendimiento (importante en WebGL).")]
    [SerializeField] protected float intervaloDeteccion = 0.2f;

    [Header("Animación (opcional)")]
    [Tooltip("Animator del modelo. Si se deja vacío se busca en los hijos.")]
    [SerializeField] protected Animator animador;

    [Tooltip("Nombre del parámetro float del Animator que recibe la velocidad.")]
    [SerializeField] protected string parametroVelocidad = "Velocidad";

    [Header("Depuración")]
    [Tooltip("Dibuja en la escena los radios y el cono de visión al seleccionar el enemigo.")]
    [SerializeField] private bool mostrarGizmos = true;

    /// <summary>Transform del jugador persistente (se obtiene por código, no por Inspector).</summary>
    protected Transform jugador;

    /// <summary>Estado actual de la IA.</summary>
    protected EstadoEnemigo estadoActual = EstadoEnemigo.Patrullando;

    private float temporizadorDeteccion;
    private float ultimoMomentoVisto;
    private int hashVelocidad;
    private bool usarParametroVelocidad;

    /// <summary>Estado actual (solo lectura para otros scripts).</summary>
    public EstadoEnemigo EstadoActual => estadoActual;

    /// <summary>Velocidad con la que se mueve el enemigo (para el Animator).</summary>
    protected abstract float VelocidadActual { get; }

    // ───────────────────────── Ciclo de vida ─────────────────────────

    protected virtual void Start()
    {
        BuscarJugador();
        ConfigurarAnimador();

        // Desfase aleatorio para que no todos los enemigos revisen la visión en el mismo frame.
        temporizadorDeteccion = Random.Range(0f, intervaloDeteccion);

        estadoActual = EstadoEnemigo.Patrullando;
        AlCambiarEstado(estadoActual);
    }

    protected virtual void Update()
    {
        temporizadorDeteccion -= Time.deltaTime;
        if (temporizadorDeteccion <= 0f)
        {
            temporizadorDeteccion = intervaloDeteccion;
            EvaluarDeteccion();
        }

        // Si el jugador desaparece (cambio de escena, muerte), se vuelve a patrullar.
        if (estadoActual == EstadoEnemigo.Persiguiendo && jugador == null)
            CambiarEstado(EstadoEnemigo.Patrullando);

        if (estadoActual == EstadoEnemigo.Patrullando)
            ActualizarPatrulla();
        else
            ActualizarPersecucion();

        ActualizarAnimacion();
    }

    protected virtual void OnValidate()
    {
        radioPerdida = Mathf.Max(radioPerdida, radioDeteccion);
        intervaloDeteccion = Mathf.Max(0.02f, intervaloDeteccion);
    }

    // ───────────────────────── Métodos que define cada enemigo ─────────────────────────

    /// <summary>Se llama cada frame mientras el enemigo patrulla.</summary>
    protected abstract void ActualizarPatrulla();

    /// <summary>Se llama cada frame mientras el enemigo persigue al jugador.</summary>
    protected abstract void ActualizarPersecucion();

    /// <summary>Se llama una vez al entrar a un estado nuevo.</summary>
    protected virtual void AlCambiarEstado(EstadoEnemigo nuevoEstado) { }

    // ───────────────────────── API pública ─────────────────────────

    /// <summary>
    /// Obliga al enemigo a perseguir al jugador aunque no lo vea
    /// (por ejemplo, cuando el jugador lo golpea por la espalda).
    /// </summary>
    public void Alertar()
    {
        if (jugador == null) BuscarJugador();
        if (jugador == null) return;

        ultimoMomentoVisto = Time.time;
        CambiarEstado(EstadoEnemigo.Persiguiendo);
    }

    // ───────────────────────── Detección ─────────────────────────

    private void EvaluarDeteccion()
    {
        if (jugador == null)
        {
            BuscarJugador();
            if (jugador == null) return;
        }

        if (estadoActual == EstadoEnemigo.Patrullando)
        {
            if (PuedeVerJugador(radioDeteccion, true))
                CambiarEstado(EstadoEnemigo.Persiguiendo);
        }
        else
        {
            // Mientras persigue no usa el cono de visión: solo distancia y línea de visión.
            if (PuedeVerJugador(radioPerdida, false))
                ultimoMomentoVisto = Time.time;
            else if (Time.time - ultimoMomentoVisto >= tiempoOlvido)
                CambiarEstado(EstadoEnemigo.Patrullando);
        }
    }

    /// <summary>
    /// Revisa si el jugador está dentro del radio, dentro del cono de visión (opcional)
    /// y sin obstáculos en medio.
    /// </summary>
    protected bool PuedeVerJugador(float radio, bool usarCono)
    {
        if (jugador == null) return false;

        Vector3 origen = transform.position + Vector3.up * alturaOjos;
        Vector3 destino = jugador.position + Vector3.up * alturaObjetivo;
        Vector3 direccion = destino - origen;
        float distancia = direccion.magnitude;

        if (distancia > radio) return false;
        if (distancia < 0.01f) return true;

        if (usarCono && anguloVision < 360f)
        {
            // El ángulo se mide en horizontal para que el volador también vea hacia abajo.
            Vector3 direccionPlana = Vector3.ProjectOnPlane(direccion, Vector3.up);
            if (Vector3.Angle(transform.forward, direccionPlana) > anguloVision * 0.5f)
                return false;
        }

        // Línea de visión: si lo primero que toca el rayo es el jugador, lo ve.
        if (Physics.Raycast(origen, direccion / distancia, out RaycastHit golpe, distancia,
                            capasObstaculos, QueryTriggerInteraction.Ignore))
        {
            return EsJugador(golpe.transform);
        }

        return true;
    }

    /// <summary>Indica si un transform pertenece al jugador (él mismo o un hijo).</summary>
    protected bool EsJugador(Transform t)
    {
        return jugador != null && t != null && (t == jugador || t.IsChildOf(jugador));
    }

    // ───────────────────────── Utilidades ─────────────────────────

    /// <summary>Cambia de estado y avisa a la clase hija.</summary>
    protected void CambiarEstado(EstadoEnemigo nuevoEstado)
    {
        if (estadoActual == nuevoEstado) return;

        estadoActual = nuevoEstado;
        if (nuevoEstado == EstadoEnemigo.Persiguiendo)
            ultimoMomentoVisto = Time.time;

        AlCambiarEstado(nuevoEstado);
    }

    /// <summary>Gira suavemente al enemigo en horizontal hacia un punto.</summary>
    protected void GirarHacia(Vector3 punto, float velocidadGiro)
    {
        Vector3 direccion = punto - transform.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.001f) return;

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadGiro * Time.deltaTime);
    }

    /// <summary>Obtiene al jugador persistente (nunca por referencia del Inspector).</summary>
    protected void BuscarJugador()
    {
        var jugadorPersistente = JugadorPersistente.Obtener();
        if (jugadorPersistente != null)
            jugador = jugadorPersistente.transform;
    }

    private void ConfigurarAnimador()
    {
        if (animador == null) animador = GetComponentInChildren<Animator>();

        usarParametroVelocidad = false;
        if (animador == null || string.IsNullOrEmpty(parametroVelocidad)) return;

        hashVelocidad = Animator.StringToHash(parametroVelocidad);
        foreach (AnimatorControllerParameter parametro in animador.parameters)
        {
            if (parametro.nameHash == hashVelocidad && parametro.type == AnimatorControllerParameterType.Float)
            {
                usarParametroVelocidad = true;
                break;
            }
        }
    }

    private void ActualizarAnimacion()
    {
        if (usarParametroVelocidad)
            animador.SetFloat(hashVelocidad, VelocidadActual);
    }

    // ───────────────────────── Gizmos ─────────────────────────

    protected virtual void OnDrawGizmosSelected()
    {
        if (!mostrarGizmos) return;

        Vector3 ojos = transform.position + Vector3.up * alturaOjos;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(ojos, radioDeteccion);

        Gizmos.color = new Color(1f, 0.45f, 0f);
        Gizmos.DrawWireSphere(ojos, radioPerdida);

        if (anguloVision < 360f)
        {
            Gizmos.color = Color.cyan;
            Vector3 izquierda = Quaternion.Euler(0f, -anguloVision * 0.5f, 0f) * transform.forward;
            Vector3 derecha = Quaternion.Euler(0f, anguloVision * 0.5f, 0f) * transform.forward;
            Gizmos.DrawLine(ojos, ojos + izquierda * radioDeteccion);
            Gizmos.DrawLine(ojos, ojos + derecha * radioDeteccion);
        }

        if (Application.isPlaying && jugador != null && estadoActual == EstadoEnemigo.Persiguiendo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ojos, jugador.position + Vector3.up * alturaObjetivo);
        }
    }
}
