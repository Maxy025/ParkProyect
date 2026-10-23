using UnityEngine;

/// <summary>
/// Enemigo que vuela libremente en 3D (no usa NavMesh).
/// Patrulla por el aire, esquiva obstáculos con un SphereCast y, cuando ve al jugador,
/// se acerca volando por encima de él.
/// </summary>
public class EnemigoVolador : EnemigoIA
{
    [Header("Patrulla aérea")]
    [Tooltip("Puntos (en el aire) que recorre en orden. Si se deja vacío, vuela al azar alrededor de su posición inicial.")]
    [SerializeField] private Transform[] puntosPatrulla;

    [Tooltip("Radio horizontal de vuelo aleatorio cuando no hay puntos de patrulla.")]
    [SerializeField] private float radioPatrulla = 8f;

    [Tooltip("Cuánto puede subir o bajar respecto a su altura inicial al patrullar al azar.")]
    [SerializeField] private float variacionAltura = 1.5f;

    [Tooltip("Velocidad al patrullar.")]
    [SerializeField] private float velocidadPatrulla = 3f;

    [Tooltip("Segundos que espera flotando al llegar a cada punto.")]
    [SerializeField] private float esperaEnPunto = 1f;

    [Tooltip("Distancia para considerar que llegó a un punto.")]
    [SerializeField] private float distanciaLlegada = 0.5f;

    [Header("Persecución")]
    [Tooltip("Velocidad al perseguir al jugador.")]
    [SerializeField] private float velocidadPersecucion = 5.5f;

    [Tooltip("Altura sobre los pies del jugador a la que intenta volar.")]
    [SerializeField] private float alturaSobreJugador = 1.5f;

    [Tooltip("Distancia a la que se detiene del punto de persecución mientras espera para atacar. Debe ser menor que el Rango Ataque.")]
    [SerializeField] private float distanciaDetenerse = 2f;

    [Header("Movimiento")]
    [Tooltip("Qué tan rápido cambia su velocidad (suaviza arranques y frenadas).")]
    [SerializeField] private float aceleracion = 8f;

    [Tooltip("Qué tan rápido gira hacia donde se mueve.")]
    [SerializeField] private float velocidadGiro = 6f;

    [Tooltip("Altura mínima que mantiene sobre el suelo.")]
    [SerializeField] private float alturaMinimaSuelo = 1f;

    [Header("Evasión de obstáculos")]
    [Tooltip("Radio de la esfera con la que detecta obstáculos (similar al tamaño del enemigo).")]
    [SerializeField] private float radioEvasion = 0.5f;

    [Tooltip("Distancia hacia adelante en la que busca obstáculos.")]
    [SerializeField] private float distanciaEvasion = 2.5f;

    [Header("Flotación visual (opcional)")]
    [Tooltip("Hijo con el modelo 3D. Se mueve arriba y abajo para dar sensación de vuelo.")]
    [SerializeField] private Transform modeloVisual;

    [SerializeField] private float amplitudFlotacion = 0.25f;
    [SerializeField] private float frecuenciaFlotacion = 2f;

    private Vector3 velocidad;
    private Vector3 posicionInicial;
    private Vector3[] posicionesPatrulla;
    private Vector3 destinoPatrulla;
    private int indicePunto;
    private bool esperando;
    private float temporizadorEspera;
    private Vector3 posicionVisualInicial;
    private float desfaseFlotacion;

    protected override float VelocidadActual => velocidad.magnitude;

    // El volador se mueve en 3D: la embestida puede ser en picada.
    protected override bool MovimientoSoloHorizontal => false;

    protected override void Start()
    {
        posicionInicial = transform.position;

        posicionesPatrulla = puntosPatrulla == null
            ? new Vector3[0]
            : System.Array.ConvertAll(System.Array.FindAll(puntosPatrulla, p => p != null), p => p.position);

        if (modeloVisual != null) posicionVisualInicial = modeloVisual.localPosition;
        desfaseFlotacion = Random.Range(0f, Mathf.PI * 2f);

        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        AnimarFlotacion();
    }

    protected override void AlCambiarEstado(EstadoEnemigo nuevoEstado)
    {
        if (nuevoEstado == EstadoEnemigo.Patrullando)
        {
            esperando = false;
            ElegirSiguienteDestino();
        }
    }

    // ───────────────────────── Patrulla ─────────────────────────

    protected override void ActualizarPatrulla()
    {
        if (esperando)
        {
            Frenar();
            temporizadorEspera -= Time.deltaTime;
            if (temporizadorEspera <= 0f)
            {
                esperando = false;
                ElegirSiguienteDestino();
            }
            return;
        }

        bool llego = MoverHacia(destinoPatrulla, velocidadPatrulla, distanciaLlegada);
        if (llego)
        {
            esperando = true;
            temporizadorEspera = esperaEnPunto;
        }
    }

    private void ElegirSiguienteDestino()
    {
        if (posicionesPatrulla.Length > 0)
        {
            destinoPatrulla = posicionesPatrulla[indicePunto];
            indicePunto = (indicePunto + 1) % posicionesPatrulla.Length;
        }
        else
        {
            Vector2 circulo = Random.insideUnitCircle * radioPatrulla;
            destinoPatrulla = posicionInicial + new Vector3(
                circulo.x,
                Random.Range(-variacionAltura, variacionAltura),
                circulo.y);
        }
    }

    // ───────────────────────── Persecución ─────────────────────────

    protected override void ActualizarPersecucion()
    {
        if (jugador == null) return;

        Vector3 objetivo = jugador.position + Vector3.up * alturaSobreJugador;
        bool cerca = MoverHacia(objetivo, velocidadPersecucion, distanciaDetenerse);

        if (cerca) GirarHacia(jugador.position, velocidadGiro);
    }

    // ───────────────────────── Movimiento del ataque ─────────────────────────

    /// <summary>Movimiento directo (embestida y retroceso), esquivando obstáculos.</summary>
    protected override void MoverDirecto(Vector3 direccion, float rapidez)
    {
        if (direccion.sqrMagnitude < 0.0001f) { Frenar(); return; }

        Vector3 direccionLibre = CalcularDireccionLibre(direccion.normalized);
        velocidad = direccionLibre * rapidez; // sin aceleración: la embestida debe sentirse rápida
        AplicarMovimiento();
    }

    protected override void DetenerMovimiento()
    {
        Frenar();
    }

    // ───────────────────────── Movimiento ─────────────────────────

    /// <summary>
    /// Mueve al enemigo hacia un destino esquivando obstáculos.
    /// Devuelve true cuando ya está a la distancia de parada.
    /// </summary>
    private bool MoverHacia(Vector3 destino, float rapidez, float distanciaParada)
    {
        Vector3 haciaDestino = destino - transform.position;
        float distancia = haciaDestino.magnitude;

        if (distancia <= distanciaParada)
        {
            Frenar();
            return true;
        }

        Vector3 direccion = CalcularDireccionLibre(haciaDestino / distancia);
        velocidad = Vector3.MoveTowards(velocidad, direccion * rapidez, aceleracion * Time.deltaTime);
        AplicarMovimiento();

        // Gira hacia donde se mueve (solo en horizontal).
        Vector3 velocidadPlana = new Vector3(velocidad.x, 0f, velocidad.z);
        if (velocidadPlana.sqrMagnitude > 0.01f)
            GirarHacia(transform.position + velocidadPlana, velocidadGiro);

        return false;
    }

    private void Frenar()
    {
        velocidad = Vector3.MoveTowards(velocidad, Vector3.zero, aceleracion * Time.deltaTime);
        AplicarMovimiento();
    }

    private void AplicarMovimiento()
    {
        Vector3 nuevaPosicion = transform.position + velocidad * Time.deltaTime;

        // Mantener una altura mínima sobre el suelo.
        if (Physics.Raycast(nuevaPosicion, Vector3.down, out RaycastHit golpe, alturaMinimaSuelo,
                            capasObstaculos, QueryTriggerInteraction.Ignore)
            && !EsJugador(golpe.transform))
        {
            nuevaPosicion.y = golpe.point.y + alturaMinimaSuelo;
            if (velocidad.y < 0f) velocidad.y = 0f;
        }

        transform.position = nuevaPosicion;
    }

    /// <summary>
    /// Si hay un obstáculo enfrente, prueba direcciones alternativas
    /// (deslizarse por la superficie, subir, ir a los lados).
    /// </summary>
    private Vector3 CalcularDireccionLibre(Vector3 direccionDeseada)
    {
        if (!HayObstaculo(direccionDeseada, out RaycastHit golpe))
            return direccionDeseada;

        Vector3 derecha = Vector3.Cross(Vector3.up, direccionDeseada);
        if (derecha.sqrMagnitude < 0.001f) derecha = transform.right;
        derecha.Normalize();

        Vector3[] alternativas =
        {
            Vector3.ProjectOnPlane(direccionDeseada, golpe.normal).normalized, // deslizarse
            (direccionDeseada + Vector3.up).normalized,                          // subir
            (direccionDeseada + derecha).normalized,                             // derecha
            (direccionDeseada - derecha).normalized,                             // izquierda
            Vector3.up                                                           // último recurso
        };

        foreach (Vector3 alternativa in alternativas)
        {
            if (alternativa.sqrMagnitude > 0.001f && !HayObstaculo(alternativa, out _))
                return alternativa;
        }

        return Vector3.up;
    }

    private bool HayObstaculo(Vector3 direccion, out RaycastHit golpe)
    {
        bool choca = Physics.SphereCast(transform.position, radioEvasion, direccion, out golpe,
                                        distanciaEvasion, capasObstaculos, QueryTriggerInteraction.Ignore);

        // El jugador no cuenta como obstáculo (es a quien queremos alcanzar).
        return choca && !EsJugador(golpe.transform);
    }

    private void AnimarFlotacion()
    {
        if (modeloVisual == null) return;

        float desplazamiento = Mathf.Sin(Time.time * frecuenciaFlotacion + desfaseFlotacion) * amplitudFlotacion;
        modeloVisual.localPosition = posicionVisualInicial + Vector3.up * desplazamiento;
    }

    // ───────────────────────── Gizmos ─────────────────────────

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + transform.forward * distanciaEvasion, radioEvasion);

        Gizmos.color = Color.green;
        if (puntosPatrulla != null && puntosPatrulla.Length > 0)
        {
            foreach (Transform p in puntosPatrulla)
                if (p != null) Gizmos.DrawSphere(p.position, 0.25f);
        }
        else
        {
            Vector3 centro = Application.isPlaying ? posicionInicial : transform.position;
            Gizmos.DrawWireCube(centro, new Vector3(radioPatrulla * 2f, variacionAltura * 2f, radioPatrulla * 2f));
        }
    }
}
