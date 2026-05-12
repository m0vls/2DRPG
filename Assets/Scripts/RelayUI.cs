using Mirror;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Utp;

public class RelayUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinInputField;
    private UtpTransport transport;
    //public TMP_Text codeDisplay;

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
    }

    public void HostGame()
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

    public void JoinGame()
    {
        string code = joinInputField.text;
        if (string.IsNullOrEmpty(code)) return;

        transport.useRelay = true;

        transport.ConfigureClientWithJoinCode(code, () => {
            NetworkManager.singleton.StartClient();
        }, () => {
            Debug.LogError("Invalid Join Code");
        });
    }
}
