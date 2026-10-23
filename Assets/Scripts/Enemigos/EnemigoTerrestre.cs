using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemigo que camina por el suelo usando el NavMesh de Unity (pathfinding).
/// Patrulla entre puntos (o deambula al azar) y, cuando ve al jugador, se acerca a él.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemigoTerrestre : EnemigoIA
{
    [Header("Patrulla")]
    [Tooltip("Puntos que recorre en orden. Si se deja vacío, deambula al azar alrededor de su posición inicial.")]
    [SerializeField] private Transform[] puntosPatrulla;

    [Tooltip("Radio en el que deambula cuando no tiene puntos de patrulla.")]
    [SerializeField] private float radioDeambular = 8f;

    [Tooltip("Velocidad al patrullar.")]
    [SerializeField] private float velocidadPatrulla = 2f;

    [Tooltip("Segundos que espera al llegar a cada punto.")]
    [SerializeField] private float esperaEnPunto = 1.5f;

    [Header("Persecución")]
    [Tooltip("Velocidad al perseguir al jugador.")]
    [SerializeField] private float velocidadPersecucion = 4.5f;

    [Tooltip("Distancia a la que se detiene frente al jugador mientras espera para atacar. Debe ser un poco menor que el Rango Ataque.")]
    [SerializeField] private float distanciaDetenerse = 1.8f;

    [Tooltip("Cada cuántos segundos recalcula la ruta hacia el jugador.")]
    [SerializeField] private float intervaloRecalculo = 0.25f;

    [Tooltip("Qué tan rápido gira para mirar al jugador cuando está cerca.")]
    [SerializeField] private float velocidadGiro = 8f;

    private NavMeshAgent agente;
    private Vector3 posicionInicial;
    private Vector3[] posicionesPatrulla;
    private int indicePunto;
    private bool esperando;
    private float temporizadorEspera;
    private float temporizadorRecalculo;
    private float velocidadManual;

    protected override float VelocidadActual
    {
        get
        {
            if (agente == null) return 0f;
            return estadoActual == EstadoEnemigo.Atacando ? velocidadManual : agente.velocity.magnitude;
        }
    }

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    protected override void Start()
    {
        posicionInicial = transform.position;
        GuardarPosicionesPatrulla();

        if (!agente.isOnNavMesh)
            Debug.LogWarning($"[EnemigoTerrestre] '{name}' no está sobre un NavMesh. ¿Hiciste Bake en esta escena?", this);

        base.Start();
    }

    /// <summary>
    /// Guarda las posiciones de los puntos al iniciar, así pueden ser hijos del enemigo
    /// sin moverse con él.
    /// </summary>
    private void GuardarPosicionesPatrulla()
    {
        int cantidad = 0;
        if (puntosPatrulla != null)
            foreach (Transform p in puntosPatrulla)
                if (p != null) cantidad++;

        posicionesPatrulla = new Vector3[cantidad];
        int i = 0;
        if (puntosPatrulla != null)
            foreach (Transform p in puntosPatrulla)
                if (p != null) posicionesPatrulla[i++] = p.position;
    }

    protected override void AlCambiarEstado(EstadoEnemigo nuevoEstado)
    {
        if (!agente.isOnNavMesh) return;

        switch (nuevoEstado)
        {
            case EstadoEnemigo.Patrullando:
                agente.isStopped = false;
                agente.updateRotation = true;
                agente.speed = velocidadPatrulla;
                agente.stoppingDistance = 0.1f;
                esperando = false;
                IrAlSiguientePunto();
                break;

            case EstadoEnemigo.Persiguiendo:
                agente.isStopped = false;
                agente.updateRotation = true;
                agente.speed = velocidadPersecucion;
                agente.stoppingDistance = distanciaDetenerse;
                temporizadorRecalculo = 0f; // recalcula la ruta de inmediato
                break;

            case EstadoEnemigo.Atacando:
                // Durante el ataque el movimiento lo controla EnemigoIA con MoverDirecto.
                agente.ResetPath();
                agente.velocity = Vector3.zero;
                agente.updateRotation = false;
                velocidadManual = 0f;
                break;
        }
    }

    // ───────────────────────── Movimiento del ataque ─────────────────────────

    /// <summary>
    /// Mueve al enemigo en línea recta con NavMeshAgent.Move, que respeta los bordes
    /// del NavMesh (no se sale del mapa ni atraviesa paredes horneadas).
    /// </summary>
    protected override void MoverDirecto(Vector3 direccion, float rapidez)
    {
        if (!agente.isOnNavMesh) return;

        direccion.y = 0f;
        agente.Move(direccion.normalized * rapidez * Time.deltaTime);
        velocidadManual = rapidez;
    }

    protected override void DetenerMovimiento()
    {
        velocidadManual = 0f;
    }

    // ───────────────────────── Patrulla ─────────────────────────

    protected override void ActualizarPatrulla()
    {
        if (!agente.isOnNavMesh) return;

        if (esperando)
        {
            temporizadorEspera -= Time.deltaTime;
            if (temporizadorEspera <= 0f)
            {
                esperando = false;
                IrAlSiguientePunto();
            }
            return;
        }

        // Si la ruta no es válida, busca otro destino.
        if (agente.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            IrAlSiguientePunto();
            return;
        }

        // ¿Llegó al punto?
        if (!agente.pathPending && agente.remainingDistance <= agente.stoppingDistance + 0.1f)
        {
            esperando = true;
            temporizadorEspera = esperaEnPunto;
        }
    }

    private void IrAlSiguientePunto()
    {
        Vector3 destino;

        if (posicionesPatrulla.Length > 0)
        {
            destino = posicionesPatrulla[indicePunto];
            indicePunto = (indicePunto + 1) % posicionesPatrulla.Length;
        }
        else
        {
            ObtenerPuntoAleatorio(out destino);
        }

        agente.SetDestination(destino);
    }

    /// <summary>Busca un punto al azar sobre el NavMesh cerca de la posición inicial.</summary>
    private bool ObtenerPuntoAleatorio(out Vector3 punto)
    {
        for (int intento = 0; intento < 5; intento++)
        {
            Vector2 circulo = Random.insideUnitCircle * radioDeambular;
            Vector3 candidato = posicionInicial + new Vector3(circulo.x, 0f, circulo.y);

            if (NavMesh.SamplePosition(candidato, out NavMeshHit golpe, 3f, NavMesh.AllAreas))
            {
                punto = golpe.position;
                return true;
            }
        }

        punto = posicionInicial;
        return false;
    }

    // ───────────────────────── Persecución ─────────────────────────

    protected override void ActualizarPersecucion()
    {
        if (!agente.isOnNavMesh || jugador == null) return;

        // Recalcular la ruta cada cierto tiempo (no cada frame) para ahorrar rendimiento.
        temporizadorRecalculo -= Time.deltaTime;
        if (temporizadorRecalculo <= 0f)
        {
            temporizadorRecalculo = intervaloRecalculo;

            // Se busca el punto del NavMesh más cercano al jugador (por si está saltando).
            if (NavMesh.SamplePosition(jugador.position, out NavMeshHit golpe, 2.5f, NavMesh.AllAreas))
                agente.SetDestination(golpe.position);
        }

        // Cuando ya está cerca, el agente deja de girar solo; lo giramos para que mire al jugador.
        float distancia = Vector3.Distance(transform.position, jugador.position);
        if (distancia <= distanciaDetenerse + 0.3f)
            GirarHacia(jugador.position, velocidadGiro);
    }

    // ───────────────────────── Gizmos ─────────────────────────

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.green;
        if (puntosPatrulla != null && puntosPatrulla.Length > 0)
        {
            for (int i = 0; i < puntosPatrulla.Length; i++)
            {
                if (puntosPatrulla[i] == null) continue;
                Gizmos.DrawSphere(puntosPatrulla[i].position, 0.25f);

                Transform siguiente = puntosPatrulla[(i + 1) % puntosPatrulla.Length];
                if (siguiente != null) Gizmos.DrawLine(puntosPatrulla[i].position, siguiente.position);
            }
        }
        else
        {
            Vector3 centro = Application.isPlaying ? posicionInicial : transform.position;
            Gizmos.DrawWireSphere(centro, radioDeambular);
        }
    }
}
