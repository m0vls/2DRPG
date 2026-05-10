using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject[] buttons;
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

        TitleText.DOMoveY(TitleText.position.y + 300f, 1.2f).SetEase(Ease.InOutQuart);

        StartCoroutine(SpawnButtons());
    }

    public IEnumerator SpawnButtons()
    {
        yield return new WaitForSeconds(1f);

        float delay = 0;

        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject btn = buttons[i];

            btn.SetActive(true);

            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            cg.DOFade(1, 0.5f).SetDelay(delay);

            btn.transform.DOMoveX(btn.transform.position.x - 250f, 0.8f)
                .SetEase(Ease.InOutBack)
                .SetDelay(delay);

            delay += 0.15f;
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Hub");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
