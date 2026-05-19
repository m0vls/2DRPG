using Mirror;
using TMPro;
using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private UpgradeUI upgradeUI;
    public TMP_Text codeText;

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        upgradeUI.gameObject.SetActive(false);
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
