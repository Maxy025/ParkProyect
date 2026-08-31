using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI del menú de habilidades: 3 ranuras en el mismo orden que GestorHabilidades.
/// Resalta la habilidad equipada, oscurece las bloqueadas y se actualiza
/// automáticamente al presionar Q/E o al desbloquear una nueva habilidad.
/// </summary>
public class MenuHabilidadesUI : MonoBehaviour
{
    [System.Serializable]
    public class RanuraHabilidadUI
    {
        public Image imagenIcono;
        public GameObject superposicionBloqueada;
        public GameObject resaltadoSeleccionado;
    }

    [SerializeField] private GestorHabilidades gestorHabilidades;
    [SerializeField] private RanuraHabilidadUI[] ranuras = new RanuraHabilidadUI[3];

    [Header("Colores")]
    [SerializeField] private Color colorDesbloqueado = Color.white;
    [SerializeField] private Color colorBloqueado = new Color(1f, 1f, 1f, 0.3f);

    private void OnEnable()
    {
        if (gestorHabilidades == null) return;
        gestorHabilidades.AlEquiparHabilidad.AddListener(ManejarHabilidadEquipada);
        gestorHabilidades.AlDesbloquearHabilidad.AddListener(ManejarHabilidadDesbloqueada);
    }

    private void OnDisable()
    {
        if (gestorHabilidades == null) return;
        gestorHabilidades.AlEquiparHabilidad.RemoveListener(ManejarHabilidadEquipada);
        gestorHabilidades.AlDesbloquearHabilidad.RemoveListener(ManejarHabilidadDesbloqueada);
    }

    private void Start()
    {
        for (int i = 0; i < ranuras.Length; i++)
        {
            Habilidad habilidad = gestorHabilidades != null ? gestorHabilidades.ObtenerHabilidad(i) : null;
            if (habilidad != null && habilidad.icono != null && ranuras[i].imagenIcono != null)
                ranuras[i].imagenIcono.sprite = habilidad.icono;

            ActualizarEstadoBloqueoRanura(i, gestorHabilidades != null && gestorHabilidades.EstaDesbloqueada(i));
            EstablecerSeleccion(i, false);
        }
    }

    private void ManejarHabilidadEquipada(int indice, Habilidad habilidad)
    {
        for (int i = 0; i < ranuras.Length; i++)
            EstablecerSeleccion(i, i == indice);
    }

    private void ManejarHabilidadDesbloqueada(int indice)
    {
        ActualizarEstadoBloqueoRanura(indice, true);
    }

    private void ActualizarEstadoBloqueoRanura(int indice, bool desbloqueada)
    {
        if (indice < 0 || indice >= ranuras.Length || ranuras[indice] == null) return;

        RanuraHabilidadUI ranura = ranuras[indice];
        if (ranura.superposicionBloqueada != null) ranura.superposicionBloqueada.SetActive(!desbloqueada);
        if (ranura.imagenIcono != null) ranura.imagenIcono.color = desbloqueada ? colorDesbloqueado : colorBloqueado;
    }

    private void EstablecerSeleccion(int indice, bool seleccionada)
    {
        if (indice < 0 || indice >= ranuras.Length || ranuras[indice] == null) return;
        if (ranuras[indice].resaltadoSeleccionado != null)
            ranuras[indice].resaltadoSeleccionado.SetActive(seleccionada);
    }
}
