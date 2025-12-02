using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    private void Start()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
            if (slider == null)
            {
                Debug.LogError("Slider not found on Healthbar!");
                enabled = false;
                return;
            }
        }

        // CORRECTION: Vérifier si fill existe
        if (fill == null && slider != null)
        {
            fill = slider.fillRect?.GetComponent<Image>();
            if (fill == null)
            {
                Debug.LogWarning("Fill image not found on Healthbar!");
            }
        }
    }

    public void SetMaxHealth(int health)
    {
        if (slider != null)
        {
            slider.maxValue = health;
            slider.value = health;

            // CORRECTION: Vérifier si fill existe avant de l'utiliser
            if (fill != null && gradient != null)
            {
                fill.color = gradient.Evaluate(1f);
            }
        }
    }

    public void SetHealth(int health)
    {
        if (slider != null)
        {
            slider.value = health;

            // CORRECTION: Vérifier si fill et gradient existent avant de les utiliser
            if (fill != null && gradient != null)
            {
                fill.color = gradient.Evaluate(slider.normalizedValue);
            }
        }
    }
}