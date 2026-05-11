using Mirror;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Utp; // Пространство имен из вашего кода

public class RelayUI : MonoBehaviour
{
    private UtpTransport transport;
    public TMP_InputField joinInputField;
    public TMP_Text codeDisplay;

    private async void Awake()
    {
        // 1. Инициализация сервисов Unity
        await UnityServices.InitializeAsync();

        // 2. Анонимный вход (обязательно для Relay)
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    void Start()
    {
        transport = RPGNetworkManager.singleton.GetComponent<UtpTransport>();
    }

    // Навесьте на кнопку "Host"
    public void HostGame()
    {
        // 1. Включаем режим Relay в транспорте
        transport.useRelay = true;

        // 2. Выделяем сервер (на 4 игрока, регион null - выберет ближайший)
        transport.AllocateRelayServer(4, null, (joinCode) => {
            // Если успех:
            //codeDisplay.text = joinCode; // Показываем код
            NetworkManager.singleton.StartHost(); // Запускаем хост Mirror
            Debug.Log($"Host started with code: {joinCode}");
        }, () => {
            Debug.LogError("Failed to allocate Relay server");
        });
    }

    // Навесьте на кнопку "Join"
    public void JoinGame()
    {
        string code = joinInputField.text;
        if (string.IsNullOrEmpty(code)) return;

        transport.useRelay = true;

        // Конфигурируем клиент по коду
        transport.ConfigureClientWithJoinCode(code, () => {
            // Если код верный и данные получены:
            NetworkManager.singleton.StartClient(); // Запускаем клиент Mirror
        }, () => {
            Debug.LogError("Invalid Join Code");
        });
    }
}
