using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class MainMenu : MonoBehaviour
{
    [Header("Коллекции UI")]
    [SerializeField] private GameObject[] mainMenuButtons;
    [SerializeField] private GameObject[] connectMenuButtons;
    [SerializeField] private GameObject[] accountMenuButtons;
    [SerializeField] private GameObject[] accountRunsList;

    private GameObject[] currentUI;

    [Header("Параметры анимации")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float moveDuration = 0.8f;
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private float delayBetweenElements = 0.15f;

    [Header("Название игры")]
    [SerializeField] private Transform TitleText;
    [SerializeField] private CanvasGroup pressAnyButtonText;

    [Header("Приветствие")]
    [SerializeField] private TMP_Text welcomeText;

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

        
        UpdateWelcomeText();
        welcomeText.DOFade(1f, 1f);

        StartCoroutine(AnimateButtons(mainMenuButtons, 0f, true));
    }

    public IEnumerator AnimateButtons(GameObject[] elements, float targetPos, bool show)
    {
        foreach (var btn in elements)
        {
            CanvasGroup group = btn.GetComponent<CanvasGroup>();
            if (group != null) group.blocksRaycasts = false;
        }

        float delay = 0;

        // Если show = true, идем от 0 до конца. Если false — с конца к началу.
        int start = show ? 0 : elements.Length - 1;
        int end = show ? elements.Length : -1;
        int step = show ? 1 : -1;

        if (show) yield return new WaitForSeconds(startDelay);

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
            cg.DOFade(targetAlpha, fadeDuration).SetDelay(delay);
            rt.DOAnchorPosX(targetPos, moveDuration)
                .SetEase(selectedEase)
                .SetDelay(delay);

            delay += delayBetweenElements;
        }

        // Ждем завершения
        yield return new WaitForSeconds(moveDuration + delay);

        if (show)
        {
            foreach (var btn in elements)
            {
                CanvasGroup group = btn.GetComponent<CanvasGroup>();
                if (group != null) group.blocksRaycasts = true;
            }
        }

        // Если мы скрывали кнопки, выключаем их в конце
        if (!show)
        {
            foreach (var btn in elements) btn.SetActive(false);
        }
    }

    public void UpdateWelcomeText()
    {
        if (welcomeText == null) return;

        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            welcomeText.text = $"Добро пожаловать, {AuthManager.Instance.PlayerNickname}!";
        }
        else
        {
            welcomeText.text = "Вы не вошли. Чтобы войти, нажмите кнопку \"Аккаунт\"";
        }
    }

    private void SwitchMenu(GameObject[] from, GameObject[] to, float coordsFrom, float coordsTo)
    {
        StartCoroutine(AnimateButtons(from, coordsFrom, false));
        currentUI = to;
        StartCoroutine(AnimateButtons(to, coordsTo, true));
    }

    public void PlayGame()
    {
        SwitchMenu(mainMenuButtons, connectMenuButtons, 250f, 0f);
    }

    public void OpenAccount()
    {
        SwitchMenu(mainMenuButtons, accountMenuButtons, 250f, 355f);
        StartCoroutine(AnimateButtons(accountRunsList, 310f, true));
    }

    public void BackToMenu()
    {
        SwitchMenu(currentUI, mainMenuButtons, 250f, 0f);
    }
    public void BactToMenuFromAccount()
    {
        SwitchMenu(currentUI, mainMenuButtons, 605f, 0f);
        StartCoroutine(AnimateButtons(accountRunsList, -300f, false));
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
