using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Hace que un objeto del Hub "recuerde" que ya se limpió, para que no reaparezca
/// al volver de un nivel (las escenas se recargan desde cero al cambiar de escena).
///
/// Colócalo en el MISMO objeto que tenga Maleza, ZonaPeligrosa, Regable u ObjetoBasura.
/// Se suscribe solo a su evento (AlCortar, AlLimpiar, AlRegar o AlRecolectar) y guarda
/// su Id en ProgresoJuego. Al volver a la escena:
/// - Maleza y basura: se desactivan.
/// - Zona peligrosa: se vuelve a limpiar (se invoca de nuevo AlLimpiar, así se repiten sus efectos, como quitar una barrera).
/// - Regable: se vuelve a regar (se invoca de nuevo AlRegar, así la plataforma vuelve a crecer).
///
/// Úsalo solo en el Hub: en los niveles conviene que todo se reinicie al repetirlos.
/// </summary>
[DisallowMultipleComponent]
public class EstadoPersistente : MonoBehaviour
{
    [Tooltip("Id único del objeto. Si se deja vacío se genera solo con la escena, la ruta en la jerarquía y la posición. " +
             "Llénalo a mano solo si el objeto se mueve o cambia de nombre.")]
    [SerializeField] private string idPersonalizado = "";

    [Tooltip("Se invoca al cargar la escena si el objeto ya se había limpiado. " +
             "Úsalo para repetir efectos que no dependan del propio objeto (abrir una puerta, encender luces...).")]
    public UnityEvent AlRestaurarEstado;

    private string idFinal;

    private Maleza maleza;
    private ZonaPeligrosa zonaPeligrosa;
    private Regable regable;
    private ObjetoBasura basura;

    public string Id => idFinal;

    private void Awake()
    {
        idFinal = string.IsNullOrEmpty(idPersonalizado) ? GenerarId() : idPersonalizado;

        maleza = GetComponent<Maleza>();
        zonaPeligrosa = GetComponent<ZonaPeligrosa>();
        regable = GetComponent<Regable>();
        basura = GetComponent<ObjetoBasura>();
    }

    private void Start()
    {
        if (ProgresoJuego.EstaCompletado(idFinal))
        {
            RestaurarEstado();
            return;
        }

        // Se suscribe al evento de limpieza que corresponda.
        if (maleza != null)
        {
            if (maleza.AlCortar == null) maleza.AlCortar = new UnityEvent();
            maleza.AlCortar.AddListener(MarcarComoLimpio);
        }

        if (zonaPeligrosa != null)
        {
            if (zonaPeligrosa.AlLimpiar == null) zonaPeligrosa.AlLimpiar = new UnityEvent();
            zonaPeligrosa.AlLimpiar.AddListener(MarcarComoLimpio);
        }

        if (regable != null)
        {
            if (regable.AlRegar == null) regable.AlRegar = new EventoFlotante();
            regable.AlRegar.AddListener(AlSerRegado);
        }

        if (basura != null)
        {
            if (basura.AlRecolectar == null) basura.AlRecolectar = new UnityEvent();
            basura.AlRecolectar.AddListener(MarcarComoLimpio);
        }

        if (maleza == null && zonaPeligrosa == null && regable == null && basura == null)
            Debug.LogWarning($"[EstadoPersistente] '{name}' no tiene Maleza, ZonaPeligrosa, Regable ni ObjetoBasura. " +
                             "Llama a MarcarComoLimpio() desde algún evento para que guarde su estado.", this);
    }

    private void OnDestroy()
    {
        if (maleza != null) maleza.AlCortar?.RemoveListener(MarcarComoLimpio);
        if (zonaPeligrosa != null) zonaPeligrosa.AlLimpiar?.RemoveListener(MarcarComoLimpio);
        if (regable != null) regable.AlRegar?.RemoveListener(AlSerRegado);
        if (basura != null) basura.AlRecolectar?.RemoveListener(MarcarComoLimpio);
    }

    /// <summary>Guarda que este objeto ya se limpió. Se puede llamar también desde cualquier UnityEvent.</summary>
    public void MarcarComoLimpio()
    {
        ProgresoJuego.MarcarCompletado(idFinal);
    }

    // La firma (float) coincide con EventoFlotante de Regable.AlRegar.
    private void AlSerRegado(float cantidadAgua)
    {
        MarcarComoLimpio();
    }

    /// <summary>Deja el objeto en su estado "limpio" sin que el jugador tenga que repetirlo.</summary>
    private void RestaurarEstado()
    {
        AlRestaurarEstado?.Invoke();

        if (zonaPeligrosa != null) zonaPeligrosa.LimpiarPeligro();
        if (regable != null) regable.Regar(1f);

        if (maleza != null || basura != null)
            gameObject.SetActive(false);
    }

    /// <summary>Id automático: Escena/Ruta/En/Jerarquía@x,y,z</summary>
    private string GenerarId()
    {
        string ruta = transform.name;
        Transform padre = transform.parent;
        while (padre != null)
        {
            ruta = padre.name + "/" + ruta;
            padre = padre.parent;
        }

        Vector3 p = transform.position;
        return string.Format(CultureInfo.InvariantCulture, "{0}/{1}@{2:F1},{3:F1},{4:F1}",
            gameObject.scene.name, ruta, p.x, p.y, p.z);
    }
}
