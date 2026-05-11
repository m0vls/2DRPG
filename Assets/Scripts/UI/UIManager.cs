using Mirror;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Контроллеры UI")]
    [SerializeField] private PauseUI pauseUI;
    [SerializeField] private HealthBar healthBar;
    //[SerializeField] private ManaBar manaBar;
    [SerializeField] private DefeatUI defeatUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowKnightUI()
    {
        healthBar.gameObject.SetActive(true);
    }
    public void ShowMageUI()
    {
        healthBar.gameObject.SetActive(true);
    }

    public void ShowDefeat()
    {
        defeatUI.ShowDefeatScreen();
    }

    public void UpdateHealthUI(float health)
    {
        healthBar.UpdateBarUI(health);
    }
}
