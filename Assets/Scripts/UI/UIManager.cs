using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Контроллеры UI")]
    [SerializeField] private PauseUI pauseUI;
    [SerializeField] private HealthBar healthBar;
    //[SerializeField] private ManaBar manaBar;
    [SerializeField] private DefeatUI defeatUI;

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

    public void TogglePause()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu") return;

        pauseUI.Toggle();
    }

    public void SetRoomCode(string roomCode)
    {
        pauseUI.codeText.text = roomCode;
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
