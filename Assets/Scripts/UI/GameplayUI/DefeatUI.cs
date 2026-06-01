using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;

public class DefeatUI : MonoBehaviour
{
    [Header("Настройки UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 3f;
    [SerializeField] private float waitBeforeHub = 2f;
    [SerializeField] private TMP_Text titleText;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
    }
    public void ShowDefeatScreen(bool isVictory)
    {
        canvasGroup.gameObject.SetActive(true);
        if (isVictory) titleText.text = "Победа";
        else titleText.text = "Вы погибли";
        StartCoroutine(DefeatSequence());
    }

    private IEnumerator DefeatSequence()
    {
        UIManager.Instance.IsInputBlock = true;
        //Медленное появление
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1;

        yield return new WaitForSeconds(waitBeforeHub);

        if (NetworkServer.active)
        {
            //NetworkManager.singleton.ServerChangeScene("Hub");
            //временно отключаем от сервера
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }
        }

        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
        UIManager.Instance.IsInputBlock = false;
    }
}
