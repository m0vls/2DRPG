using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBar;

    private float targetFill = 1f;

    public void UpdateBarUI(int currentHealth)
    {
        targetFill = currentHealth / 100f;
    }

    void Update()
    {
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, targetFill, Time.deltaTime * 5f);
    }
}