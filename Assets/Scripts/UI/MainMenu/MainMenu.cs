using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject[] mainMenuButtons;
    [SerializeField] private GameObject[] connectMenuButtons;
    [SerializeField] private Transform TitleText;
    [SerializeField] private CanvasGroup pressAnyButtonText;

    private IDisposable anyPressKey;

    private void OnEnable()
    {
        anyPressKey = InputSystem.onAnyButtonPress.CallOnce(ctrl => StartMenu());
    }
    
    private void StartMenu()
    {
        pressAnyButtonText.DOFade(0, 0.5f)
            .OnComplete(() =>
            {
                pressAnyButtonText.gameObject.SetActive(false);
                pressAnyButtonText.DOKill();
            });

        RectTransform rt = TitleText.GetComponent<RectTransform>();
        rt.DOAnchorPosY(-200, 1.2f).SetEase(Ease.InOutQuart);
        
        StartCoroutine(AnimateButtons(mainMenuButtons, 0f, true));
    }

    /*public IEnumerator SpawnButtons()
    {
        yield return new WaitForSeconds(1f);

        float delay = 0;

        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject btn = buttons[i];

            btn.SetActive(true);

            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            cg.DOFade(1, 0.5f).SetDelay(delay);

            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.DOAnchorPosX(0f, 0.8f)
                .SetEase(Ease.InOutBack)
                .SetDelay(delay);

            delay += 0.15f;
        }
    }*/

    public IEnumerator AnimateButtons(GameObject[] elements, float targetPos, bool show)
    {
        float delay = 0;

        // Если show = true, идем от 0 до конца. Если false — с конца к началу.
        int start = show ? 0 : elements.Length - 1;
        int end = show ? elements.Length : -1;
        int step = show ? 1 : -1;

        if (show) yield return new WaitForSeconds(0.8f);

        for (int i = start; i != end; i += step)
        {
            GameObject btn = elements[i];
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            RectTransform rt = btn.GetComponent<RectTransform>();

            if (show) btn.SetActive(true);

            // Настраиваем параметры в зависимости от bool show
            float targetAlpha = show ? 1f : 0f;
            //float targetPos = show ? 0f : 250f;
            Ease selectedEase = show ? Ease.OutBack : Ease.InBack;

            // Сами анимации
            cg.DOFade(targetAlpha, 0.5f).SetDelay(delay);
            rt.DOAnchorPosX(targetPos, 0.8f)
                .SetEase(selectedEase)
                .SetDelay(delay);

            delay += 0.15f;
        }

        // Ждем завершения
        yield return new WaitForSeconds(0.8f + delay);

        // Если мы скрывали кнопки, выключаем их в конце
        if (!show)
        {
            foreach (var btn in elements) btn.SetActive(false);
        }
    }

    public void PlayGame()
    {
        //SceneManager.LoadScene("Hub");
        StartCoroutine(AnimateButtons(mainMenuButtons, 250f, false));
        StartCoroutine(AnimateButtons(connectMenuButtons, 0f, true));
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
