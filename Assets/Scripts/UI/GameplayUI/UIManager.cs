using DG.Tweening;
using System.Security.Policy;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [HideInInspector] public bool IsInputBlock = false;

    [Header("Контроллеры UI")]
    [SerializeField] private PauseUI pauseUI;
    [SerializeField] private DefeatUI defeatUI;
    [SerializeField] private SelectClassUI selectClassUI;
    [SerializeField] private PlayerStatusBar playerStatusBar;
    [SerializeField] private CurrencyUI currencyUI;
    //[SerializeField] private ManaBar manaBar;

    [Header("Настройки Fade")]
    [SerializeField] private CanvasGroup fadeGroup;

    private InputSystem_Actions uiActions;

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

    private void OnEnable()
    {
        if (uiActions == null) uiActions = new InputSystem_Actions();
        uiActions.UI.Enable();
    }

    private void Update()
    {
        if (uiActions.UI.Cancel.triggered)
        {
            TogglePause();
        }
    }

    public Tween FadeScreen(float targetAlpha, float duration)
    {
        if (fadeGroup == null) return null;

        // Убиваем текущую анимацию, если она идет, чтобы не было конфликтов
        fadeGroup.DOKill();
        return fadeGroup.DOFade(targetAlpha, duration);
    }

    public void TogglePause()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu") return;

        pauseUI.Toggle();
        IsInputBlock = !IsInputBlock;
    }

    public void SetRoomCode(string roomCode)
    {
        pauseUI.codeText.text = roomCode;
    }

    public void ShowKnightUI()
    {
        playerStatusBar.gameObject.SetActive(true);
        currencyUI.gameObject.SetActive(true);
    }
    public void ShowMageUI()
    {
        playerStatusBar.gameObject.SetActive(true);
        currencyUI.gameObject.SetActive(true);
    }
    public void ToggleSelectUI()
    {
        GameObject go = selectClassUI.gameObject;
        go.SetActive(!go.activeSelf);
    }

    public void ShowDefeat()
    {
        defeatUI.ShowDefeatScreen();
    }

    public void HideGameplayUI()
    {
        playerStatusBar.ResetUI();
        if (currencyUI != null) currencyUI.ResetUI();
        playerStatusBar.gameObject.SetActive(false);
        if (currencyUI != null) currencyUI.gameObject.SetActive(false);
    }

    public void UpdateHealthUI(float health)
    {
        playerStatusBar.UpdateHealthBarUI(health);
    }

    public void UpdateXPBarUI(float currentXp, float xpToNextLevel)
    {
        playerStatusBar.UpdateXpBarUI(currentXp, xpToNextLevel);
    }
    public void UpdateCurrencyUI(int amount)
    {
        if (currencyUI != null)
            currencyUI.UpdateCurrency(amount);
    }
}
