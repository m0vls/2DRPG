using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private UpgradeUI upgradeUI;
    [SerializeField] private HubUI hubUI;
    [SerializeField] private Button shopButton;
    public TMP_Text codeText;

    private void Start()
    {
        UpdateShopButtonVisibility();
    }

    private void UpdateShopButtonVisibility()
    {
        if (shopButton != null)
        {
            bool state = SceneManager.GetActiveScene().name == "Hub";
            shopButton.interactable = state;
            shopButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = state ? Color.white : Color.gray;
        }
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        if (gameObject.activeSelf)
            UpdateShopButtonVisibility();
        upgradeUI.gameObject.SetActive(false);
        hubUI.gameObject.SetActive(false);
    }

    public void Resume()
    {
        gameObject.SetActive(false);
        UIManager.Instance.IsInputBlock = false;
    }

    public void ToggleSkillUpgrade()
    {
        var go = upgradeUI.gameObject;
        go.SetActive(!go.activeSelf);
    }

    public void ToggleShop()
    {
        var go = hubUI.gameObject;
        bool open = !go.activeSelf;
        go.SetActive(open);
        if (open)
            hubUI.SetInteractive(false);
    }

    public void QuitToMenu()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        { 
            NetworkManager.singleton.StopClient();
        }
        UIManager.Instance.IsInputBlock = false;
        gameObject.SetActive(false);
    }

    public void CopyCodeToClipboard()
    {
        GUIUtility.systemCopyBuffer = codeText.text;
    }
}
