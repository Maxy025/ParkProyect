using UnityEngine;

/// <summary>
/// Clase base abstracta para la IA de los enemigos.
/// Se encarga de detectar al jugador (radio, cono de visión y línea de visión),
/// de cambiar entre los estados Patrullando, Persiguiendo y Atacando, y del ciclo de ataque:
/// Preparación (aviso) → Embestida (daño) → Retroceso (se separa) → Enfriamiento.
/// Cada tipo de enemigo (terrestre, volador) define cómo se mueve.
/// </summary>
public abstract class EnemigoIA : MonoBehaviour
{
    /// <summary>Estados posibles de la IA.</summary>
    public enum EstadoEnemigo { Patrullando, Persiguiendo, Atacando }

    /// <summary>Fases del ataque.</summary>
    protected enum FaseAtaque { Preparacion, Embestida, Retroceso }

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

    [Header("Ataque")]
    [Tooltip("Si se desactiva, el enemigo solo se acerca al jugador (comportamiento anterior).")]
    [SerializeField] protected bool puedeAtacar = true;

    [Tooltip("Distancia al jugador a la que empieza un ataque. Debe ser un poco mayor que la Distancia Detenerse.")]
    [SerializeField] protected float rangoAtaque = 2.2f;

    [Tooltip("Segundos que se queda quieto 'avisando' antes de lanzarse. Le da al jugador oportunidad de esquivar.")]
    [SerializeField] protected float tiempoPreparacion = 0.45f;

    [Tooltip("Velocidad de la embestida.")]
    [SerializeField] protected float velocidadEmbestida = 9f;

    [Tooltip("Duración máxima de la embestida en segundos.")]
    [SerializeField] protected float duracionEmbestida = 0.25f;

    [Tooltip("Distancia a la que la embestida se detiene para no atravesar al jugador.")]
    [SerializeField] protected float distanciaContacto = 0.8f;

    [Tooltip("Velocidad con la que se aleja del jugador después de atacar o de recibir un golpe.")]
    [SerializeField] protected float velocidadRetroceso = 4f;

    [Tooltip("Segundos que dura el retroceso.")]
    [SerializeField] protected float duracionRetroceso = 0.6f;

    [Tooltip("Segundos de espera entre ataques. Varía ±20 % para que varios enemigos no ataquen al mismo tiempo.")]
    [SerializeField] protected float tiempoEnfriamiento = 1.2f;

    [Tooltip("Si se activa, el enemigo retrocede cuando el jugador lo golpea (y se cancela su ataque).")]
    [SerializeField] protected bool retrocederAlRecibirGolpe = true;

    [Tooltip("Opcional. Collider con EnemyDamageDealer que solo se enciende durante la embestida. Si se deja vacío, el daño por contacto funciona como siempre.")]
    [SerializeField] protected Collider colliderDano;

    [Tooltip("Qué tan rápido gira hacia el jugador mientras prepara el ataque o retrocede.")]
    [SerializeField] protected float velocidadGiroAtaque = 10f;

    [Header("Animación (opcional)")]
    [Tooltip("Animator del modelo. Si se deja vacío se busca en los hijos.")]
    [SerializeField] protected Animator animador;

    [Tooltip("Nombre del parámetro float del Animator que recibe la velocidad.")]
    [SerializeField] protected string parametroVelocidad = "Velocidad";

    [Tooltip("Nombre del parámetro Trigger del Animator que se dispara al preparar un ataque.")]
    [SerializeField] protected string triggerAtaque = "Atacar";

    [Header("Depuración")]
    [Tooltip("Dibuja en la escena los radios y el cono de visión al seleccionar el enemigo.")]
    [SerializeField] private bool mostrarGizmos = true;

    /// <summary>Transform del jugador persistente (se obtiene por código, no por Inspector).</summary>
    protected Transform jugador;

    /// <summary>Estado actual de la IA.</summary>
    protected EstadoEnemigo estadoActual = EstadoEnemigo.Patrullando;

    /// <summary>Fase actual del ataque (solo tiene sentido en el estado Atacando).</summary>
    protected FaseAtaque faseAtaque;

    private float temporizadorDeteccion;
    private float ultimoMomentoVisto;
    private float temporizadorFase;
    private float siguienteAtaquePermitido;
    private Vector3 direccionEmbestida;

    private int hashVelocidad;
    private int hashAtaque;
    private bool usarParametroVelocidad;
    private bool usarTriggerAtaque;

    /// <summary>Estado actual (solo lectura para otros scripts).</summary>
    public EstadoEnemigo EstadoActual => estadoActual;

    /// <summary>Velocidad con la que se mueve el enemigo (para el Animator).</summary>
    protected abstract float VelocidadActual { get; }

    /// <summary>True si el enemigo solo se mueve en horizontal (terrestre). El volador lo cambia a false.</summary>
    protected virtual bool MovimientoSoloHorizontal => true;

    // ───────────────────────── Ciclo de vida ─────────────────────────

    protected virtual void Start()
    {
        BuscarJugador();
        ConfigurarAnimador();
        ActivarColliderDano(false);

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
        if (estadoActual != EstadoEnemigo.Patrullando && jugador == null)
            CambiarEstado(EstadoEnemigo.Patrullando);

        switch (estadoActual)
        {
            case EstadoEnemigo.Patrullando:
                ActualizarPatrulla();
                break;

            case EstadoEnemigo.Persiguiendo:
                ActualizarPersecucion();
                RevisarInicioAtaque();
                break;

            case EstadoEnemigo.Atacando:
                ActualizarAtaque();
                break;
        }

        ActualizarAnimacion();
    }

    protected virtual void OnValidate()
    {
        radioPerdida = Mathf.Max(radioPerdida, radioDeteccion);
        intervaloDeteccion = Mathf.Max(0.02f, intervaloDeteccion);
        rangoAtaque = Mathf.Max(rangoAtaque, distanciaContacto);
    }

    // ───────────────────────── Métodos que define cada enemigo ─────────────────────────

    /// <summary>Se llama cada frame mientras el enemigo patrulla.</summary>
    protected abstract void ActualizarPatrulla();

    /// <summary>Se llama cada frame mientras el enemigo persigue al jugador.</summary>
    protected abstract void ActualizarPersecucion();

    /// <summary>Mueve al enemigo en línea recta (se usa en la embestida y el retroceso).</summary>
    protected abstract void MoverDirecto(Vector3 direccion, float rapidez);

    /// <summary>Detiene el movimiento del enemigo.</summary>
    protected abstract void DetenerMovimiento();

    /// <summary>Se llama una vez al entrar a un estado nuevo.</summary>
    protected virtual void AlCambiarEstado(EstadoEnemigo nuevoEstado) { }

    // ───────────────────────── API pública ─────────────────────────

    /// <summary>
    /// Obliga al enemigo a perseguir al jugador aunque no lo vea.
    /// </summary>
    public void Alertar()
    {
        if (jugador == null) BuscarJugador();
        if (jugador == null) return;

        ultimoMomentoVisto = Time.time;
        if (estadoActual == EstadoEnemigo.Patrullando)
            CambiarEstado(EstadoEnemigo.Persiguiendo);
    }

    /// <summary>
    /// Reacción al recibir daño: persigue al jugador y, si está activado, retrocede
    /// (cancela el ataque que estuviera haciendo). Lo llama VidaEnemigo.
    /// </summary>
    public void RecibirGolpe()
    {
        if (jugador == null) BuscarJugador();
        if (jugador == null) return;

        ultimoMomentoVisto = Time.time;

        if (puedeAtacar && retrocederAlRecibirGolpe)
        {
            if (estadoActual != EstadoEnemigo.Atacando)
                CambiarEstado(EstadoEnemigo.Atacando);

            IniciarRetroceso();
        }
        else if (estadoActual == EstadoEnemigo.Patrullando)
        {
            CambiarEstado(EstadoEnemigo.Persiguiendo);
        }
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
            // Mientras persigue o ataca no usa el cono de visión: solo distancia y línea de visión.
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

    // ───────────────────────── Ataque ─────────────────────────

    private void RevisarInicioAtaque()
    {
        if (!puedeAtacar || jugador == null || Time.time < siguienteAtaquePermitido) return;
        if (DistanciaAlJugador() > rangoAtaque) return;

        CambiarEstado(EstadoEnemigo.Atacando);

        if (usarTriggerAtaque)
            animador.SetTrigger(hashAtaque);
    }

    private void ActualizarAtaque()
    {
        temporizadorFase -= Time.deltaTime;

        switch (faseAtaque)
        {
            case FaseAtaque.Preparacion:
                // Se queda quieto mirando al jugador: es el "aviso" del ataque.
                DetenerMovimiento();
                GirarHacia(jugador.position, velocidadGiroAtaque);

                if (temporizadorFase <= 0f)
                {
                    // La dirección se fija aquí: si el jugador se mueve a tiempo, esquiva.
                    direccionEmbestida = DireccionAlJugador();
                    faseAtaque = FaseAtaque.Embestida;
                    temporizadorFase = duracionEmbestida;
                    ActivarColliderDano(true);
                }
                break;

            case FaseAtaque.Embestida:
                if (DistanciaAlJugador() > distanciaContacto)
                    MoverDirecto(direccionEmbestida, velocidadEmbestida);
                else
                    DetenerMovimiento();

                if (temporizadorFase <= 0f)
                    IniciarRetroceso();
                break;

            case FaseAtaque.Retroceso:
                // Se aleja del jugador sin darle la espalda; así deja de "pegarse".
                MoverDirecto(-DireccionAlJugador(), velocidadRetroceso);
                GirarHacia(jugador.position, velocidadGiroAtaque);

                if (temporizadorFase <= 0f)
                    CambiarEstado(EstadoEnemigo.Persiguiendo);
                break;
        }
    }

    private void IniciarAtaque()
    {
        faseAtaque = FaseAtaque.Preparacion;
        temporizadorFase = tiempoPreparacion;
        ActivarColliderDano(false);
    }

    private void IniciarRetroceso()
    {
        faseAtaque = FaseAtaque.Retroceso;
        temporizadorFase = duracionRetroceso;
        ActivarColliderDano(false);
    }

    private void TerminarAtaque()
    {
        ActivarColliderDano(false);
        siguienteAtaquePermitido = Time.time + tiempoEnfriamiento * Random.Range(0.8f, 1.2f);
    }

    private void ActivarColliderDano(bool activo)
    {
        if (colliderDano != null)
            colliderDano.enabled = activo;
    }

    /// <summary>Distancia al jugador (horizontal para el terrestre, 3D al pecho para el volador).</summary>
    protected virtual float DistanciaAlJugador()
    {
        if (jugador == null) return float.MaxValue;

        Vector3 desde = transform.position;
        Vector3 hasta = jugador.position;

        if (MovimientoSoloHorizontal)
        {
            desde.y = 0f;
            hasta.y = 0f;
        }
        else
        {
            hasta += Vector3.up * alturaObjetivo;
        }

        return Vector3.Distance(desde, hasta);
    }

    /// <summary>Dirección normalizada hacia el jugador.</summary>
    protected Vector3 DireccionAlJugador()
    {
        Vector3 destino = jugador.position;
        if (!MovimientoSoloHorizontal) destino += Vector3.up * alturaObjetivo;

        Vector3 direccion = destino - transform.position;
        if (MovimientoSoloHorizontal) direccion.y = 0f;

        return direccion.sqrMagnitude > 0.0001f ? direccion.normalized : transform.forward;
    }

    // ───────────────────────── Utilidades ─────────────────────────

    /// <summary>Cambia de estado y avisa a la clase hija.</summary>
    protected void CambiarEstado(EstadoEnemigo nuevoEstado)
    {
        if (estadoActual == nuevoEstado) return;

        EstadoEnemigo anterior = estadoActual;
        estadoActual = nuevoEstado;

        if (anterior == EstadoEnemigo.Atacando) TerminarAtaque();
        if (anterior == EstadoEnemigo.Patrullando) ultimoMomentoVisto = Time.time;
        if (nuevoEstado == EstadoEnemigo.Atacando) IniciarAtaque();

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
        usarTriggerAtaque = false;
        if (animador == null) return;

        if (!string.IsNullOrEmpty(parametroVelocidad))
        {
            hashVelocidad = Animator.StringToHash(parametroVelocidad);
            usarParametroVelocidad = TieneParametro(hashVelocidad, AnimatorControllerParameterType.Float);
        }

        if (!string.IsNullOrEmpty(triggerAtaque))
        {
            hashAtaque = Animator.StringToHash(triggerAtaque);
            usarTriggerAtaque = TieneParametro(hashAtaque, AnimatorControllerParameterType.Trigger);
        }
    }

    private bool TieneParametro(int hash, AnimatorControllerParameterType tipo)
    {
        foreach (AnimatorControllerParameter parametro in animador.parameters)
            if (parametro.nameHash == hash && parametro.type == tipo)
                return true;
        return false;
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

        if (puedeAtacar)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, rangoAtaque);
        }

        if (anguloVision < 360f)
        {
            Gizmos.color = Color.cyan;
            Vector3 izquierda = Quaternion.Euler(0f, -anguloVision * 0.5f, 0f) * transform.forward;
            Vector3 derecha = Quaternion.Euler(0f, anguloVision * 0.5f, 0f) * transform.forward;
            Gizmos.DrawLine(ojos, ojos + izquierda * radioDeteccion);
            Gizmos.DrawLine(ojos, ojos + derecha * radioDeteccion);
        }

        if (Application.isPlaying && jugador != null && estadoActual != EstadoEnemigo.Patrullando)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ojos, jugador.position + Vector3.up * alturaObjetivo);
        }
    }
}
