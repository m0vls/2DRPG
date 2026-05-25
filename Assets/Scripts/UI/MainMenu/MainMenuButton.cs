using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool isImage = false;
    [SerializeField] private Color buttonSelectColor = Color.yellow;

    private Button button;
    private float targetScale = 1.2f;
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (isImage) gameObject.GetComponent<Image>().DOColor(buttonSelectColor, 0.2f).SetEase(Ease.OutQuad);
        else gameObject.GetComponentInChildren<TextMeshProUGUI>().DOColor(buttonSelectColor, 0.2f).SetEase(Ease.OutQuad);
        transform.DOScale(initialScale * targetScale, 0.1f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;
        ResetToDefault(0.2f);
    }

    private void OnDisable()
    {
        ResetToDefault(0f);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    private void ResetToDefault(float duration)
    {
        if (duration > 0f)
        {
            if (isImage) gameObject.GetComponent<Image>().DOColor(Color.white, duration).SetEase(Ease.OutQuad);
            else gameObject.GetComponentInChildren<TextMeshProUGUI>().DOColor(Color.white, duration).SetEase(Ease.OutQuad);
            transform.DOScale(initialScale, 0.1f).SetEase(Ease.OutQuad);
        }
        else
        {
            if (isImage) gameObject.GetComponent<Image>().color = Color.white;
            else gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            transform.localScale = initialScale;
        }
    }
}
