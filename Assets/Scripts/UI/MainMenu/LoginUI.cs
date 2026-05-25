using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button logoutButton;

    [Header("Ссылка на MainMenu для обновления приветствия")]
    [SerializeField] private MainMenu mainMenu;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLogin);
        registerButton.onClick.AddListener(OnRegister);
        logoutButton.onClick.AddListener(OnLogout);

        UpdateUIState();
    }

    private void OnEnable()
    {
        UpdateUIState();
    }

    private void UpdateUIState()
    {
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            logoutButton.interactable = true;
            logoutButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            SetButtonsInteractable(false);
        }
        else
        {
            logoutButton.interactable = false;
            logoutButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
            SetButtonsInteractable(true);
        }

        if (statusText != null)
            statusText.text = "";
    }

    private void SetStatus(string msg, Color color)
    {
        if (statusText == null) return;
        statusText.text = msg;
        statusText.color = color;
    }

    private void OnLogin()
    {
        var nick = nicknameInput.text.Trim();
        var pass = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(nick) || string.IsNullOrEmpty(pass))
        {
            SetStatus("Заполните все поля", Color.red);
            return;
        }

        SetStatus("Вход...", Color.white);
        SetButtonsInteractable(false);

        StartCoroutine(ApiService.Instance.Login(nick, pass,
            response =>
            {
                AuthManager.Instance.SaveSession(response);
                if (MetaProgression.Instance != null)
                    MetaProgression.Instance.LoadProgress();
                SetStatus("", Color.white);
                UpdateUIState();
                RefreshMainMenu();
                SetButtonsInteractable(true);
            },
            error =>
            {
                SetStatus(error, Color.red);
                SetButtonsInteractable(true);
            }
        ));
    }

    private void OnRegister()
    {
        var nick = nicknameInput.text.Trim();
        var pass = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(nick) || string.IsNullOrEmpty(pass))
        {
            SetStatus("Заполните все поля", Color.red);
            return;
        }

        if (nick.Length < 2 || nick.Length > 32)
        {
            SetStatus("Nickname от 2 до 32 символов", Color.red);
            return;
        }

        if (pass.Length < 4)
        {
            SetStatus("Пароль минимум 4 символа", Color.red);
            return;
        }

        SetStatus("Регистрация...", Color.white);
        SetButtonsInteractable(false);

        StartCoroutine(ApiService.Instance.Register(nick, pass,
            response =>
            {
                AuthManager.Instance.SaveSession(response);
                if (MetaProgression.Instance != null)
                    MetaProgression.Instance.LoadProgress();
                SetStatus("", Color.white);
                UpdateUIState();
                RefreshMainMenu();
                SetButtonsInteractable(true);
            },
            error =>
            {
                SetStatus(error, Color.red);
                SetButtonsInteractable(true);
            }
        ));
    }

    private void OnLogout()
    {
        AuthManager.Instance.ClearSession();
        UpdateUIState();
        RefreshMainMenu();
    }

    private void RefreshMainMenu()
    {
        if (mainMenu != null)
        {
            mainMenu.UpdateWelcomeText();
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        loginButton.interactable = state;
        registerButton.interactable = state;
        loginButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = state ? Color.white : Color.gray;
        registerButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = state ? Color.white : Color.gray;
    }
}
