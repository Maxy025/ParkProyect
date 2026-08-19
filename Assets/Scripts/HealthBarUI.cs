using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la barra de vida en pantalla (esquina superior derecha).
/// Se suscribe a PlayerHealth y actualiza el relleno de la barra con una
/// transición suave y un degradado de color (verde -> rojo).
///
/// Configuración recomendada del RectTransform para la esquina superior derecha:
///   Anchor Min = (1, 1), Anchor Max = (1, 1), Pivot = (1, 1)
///   Posición (offset) por ejemplo: X = -20, Y = -20
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;
    [Tooltip("Image con Image Type = Filled (Horizontal). Alternativa a Slider.")]
    [SerializeField] private Image fillImage;
    [Tooltip("Alternativa a Image: usa un Slider estándar de Unity UI.")]
    [SerializeField] private Slider healthSlider;

    [Header("Suavizado de la barra")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float smoothSpeed = 6f;

    [Header("Color según cantidad de vida")]
    [SerializeField] private bool useColorGradient = true;
    [SerializeField] private Color fullHealthColor = new Color(0.25f, 0.85f, 0.25f);
    [SerializeField] private Color lowHealthColor = new Color(0.85f, 0.15f, 0.15f);

    private float targetFill = 1f;

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
    }

    private void Update()
    {
        if (!smoothTransition) return;

        if (fillImage != null && Mathf.Abs(fillImage.fillAmount - targetFill) > 0.001f)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, smoothSpeed * Time.deltaTime);
            if (useColorGradient)
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, fillImage.fillAmount);
        }

        if (healthSlider != null && Mathf.Abs(healthSlider.value - targetFill) > 0.001f)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetFill, smoothSpeed * Time.deltaTime);
        }
    }

    private void UpdateHealthBar(float current, float max)
    {
        targetFill = max > 0 ? current / max : 0f;

        if (!smoothTransition)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = targetFill;
                if (useColorGradient)
                    fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, targetFill);
            }
            if (healthSlider != null)
                healthSlider.value = targetFill;
        }
    }
}
