using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool isImage = false;
    [SerializeField] private Color buttonSelectColor = Color.yellow;

    private float targetScale = 1.2f;
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isImage) gameObject.GetComponent<Image>().DOColor(buttonSelectColor, 0.2f).SetEase(Ease.OutQuad);
        else gameObject.GetComponentInChildren<TextMeshProUGUI>().DOColor(buttonSelectColor, 0.2f).SetEase(Ease.OutQuad);
        transform.DOScale(initialScale * targetScale, 0.1f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isImage) gameObject.GetComponent<Image>().DOColor(Color.white, 0.2f).SetEase(Ease.OutQuad);
        else gameObject.GetComponentInChildren<TextMeshProUGUI>().DOColor(Color.white, 0.2f).SetEase(Ease.OutQuad);
        transform.DOScale(initialScale, 0.1f).SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
