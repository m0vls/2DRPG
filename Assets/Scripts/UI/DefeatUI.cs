using Mirror;
using System.Collections;
using UnityEngine;

public class DefeatUI : MonoBehaviour
{
    [Header("Настройки UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 3f; // Скорость появления
    [SerializeField] private float waitBeforeHub = 2f; // Пауза после появления

    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
    }
    public void ShowDefeatScreen()
    {
        canvasGroup.gameObject.SetActive(true);
        StartCoroutine(DefeatSequence());
    }

    private IEnumerator DefeatSequence()
    {
        //Медленное появление (Fade In)
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1;

        // 2. Ждем немного, чтобы игроки успели осознать потерю
        yield return new WaitForSeconds(waitBeforeHub);

        // 3. Только сервер инициирует смену сцены обратно в Хаб
        if (NetworkServer.active)
        {
            // Используем имя вашей сцены хаба
            NetworkManager.singleton.ServerChangeScene("Hub");
        }
    }
}
