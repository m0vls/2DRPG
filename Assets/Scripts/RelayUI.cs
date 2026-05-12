using DG.Tweening;
using Mirror;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Utp;

public class RelayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField joinInputField;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text inputPlaceholder; 
    [SerializeField] private TMP_Text connectButtonText;

    private UtpTransport transport;
    private bool isLocal = false;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    void Start()
    {
        transport = RPGNetworkManager.singleton.GetComponent<UtpTransport>();
        UpdateUIState();
    }

    public void ToggleConnectionMode()
    {
        isLocal = !isLocal;

        // Анимация текста при переключении (из MainMenu style)
        statusText.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);

        UpdateUIState();
    }

    private void UpdateUIState()
    {
        if (isLocal)
        {
            statusText.text = "Игра по локальной сети: Да";
            inputPlaceholder.text = "127.0.0.0";
            connectButtonText.text = "Подключиться по IP";
        }
        else
        {
            statusText.text = "Игра по локальной сети: Нет";
            inputPlaceholder.text = "Код";
            connectButtonText.text = "Подключиться по коду";
        }
    }

    public void HostGame()
    {

        if (isLocal)
        {
            transport.useRelay = false;
            NetworkManager.singleton.StartHost();
            
            UIManager.Instance.SetRoomCode("LAN Server");
        }
        else
        {
            transport.useRelay = true;

            transport.AllocateRelayServer(2, null, (joinCode) => {
                UIManager.Instance.SetRoomCode(joinCode);
                NetworkManager.singleton.StartHost();
                Debug.Log($"Host started with code: {joinCode}");
            }, () => {
                Debug.LogError("Failed to allocate Relay server");
            });
        }   
    }

    public void JoinGame()
    {
        string input = joinInputField.text;

        if (isLocal)
        {
            // Локальное подключение
            transport.useRelay = false;
            NetworkManager.singleton.networkAddress = string.IsNullOrEmpty(input) ? "localhost" : input;
            NetworkManager.singleton.StartClient();
        }
        else
        {
            if (string.IsNullOrEmpty(input)) return;

            transport.useRelay = true;

            transport.ConfigureClientWithJoinCode(input, () => {
                NetworkManager.singleton.StartClient();
            }, () => {
                Debug.LogError("Invalid Join Code");
            });
        }
        
    }
}
