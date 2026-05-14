using Mirror;
using TMPro;
using UnityEngine;

public class PauseUI : MonoBehaviour
{
    public TMP_Text codeText;

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void Resume()
    {
        gameObject.SetActive(false);
        UIManager.Instance.IsPauseMenu = false;
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

        gameObject.SetActive(false);
    }

    public void CopyCodeToClipboard()
    {
        GUIUtility.systemCopyBuffer = codeText.text;
    }
}
