using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class DamageOverlay : MonoBehaviour
{
    [SerializeField] float fadeInDuration = 0.1f;
    [SerializeField] float holdDuration = 0.1f;
    [SerializeField] float fadeOutDuration = 0.6f;
    
    private CanvasGroup canvasGroup;
    private Sequence currentTween;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowDamage(int damage)
    {
        currentTween?.Kill();

        var fadeIn = canvasGroup.DOFade(1f, fadeInDuration);
        var fadeOut = canvasGroup.DOFade(0f, fadeOutDuration);

        currentTween = DOTween.Sequence()
            .Append(fadeIn)
            .AppendInterval(holdDuration)
            .Append(fadeOut);
    }
}