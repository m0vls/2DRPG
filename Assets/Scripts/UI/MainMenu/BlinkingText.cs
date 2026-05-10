using DG.Tweening;
using UnityEngine;

public class BlinkingText : MonoBehaviour
{
    [SerializeField] private float duration = 1f; 
    [SerializeField] private float minAlpha = 0.2f; 
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        canvasGroup.DOFade(minAlpha, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine); 
    }
}
