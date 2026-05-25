using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBar : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image xpBar;

    private float targetFillHp = 1f;
    private float targetFillXp = 0f;

    public void ResetUI()
    {
        targetFillHp = 1f;
        targetFillXp = 0f;
        healthBar.fillAmount = 1f;
        xpBar.fillAmount = 0f;
    }

    public void UpdateHealthBarUI(float currentHealth)
    {
        targetFillHp = currentHealth / 100f;
    }
    public void UpdateXpBarUI(float currentXp, float xpToNextLevel)
    {
        targetFillXp = currentXp / xpToNextLevel;
    }

    void Update()
    {
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, targetFillHp, Time.deltaTime * 5f);
        xpBar.fillAmount = Mathf.Lerp(xpBar.fillAmount, targetFillXp, Time.deltaTime * 5f);
    }
}
