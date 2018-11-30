using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class FadeZoom
{
    [SerializeField] float transitionDuration = 0.1f;
    [SerializeField] float fadedOutScale = 2f;

    private Sequence currentTransition;
    
    public Sequence FadeIn(CanvasGroup canvasGroup, Transform transform)
    {        
        currentTransition?.Kill(complete: false);
        
        canvasGroup.DOKill(complete: true);
        var fade = canvasGroup
            .DOFade(1f, transitionDuration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(isIndependentUpdate: true);

        transform.DOKill(complete: true);
        transform.localScale = Vector3.one;
        var zoom = transform
            .DOScale(fadedOutScale, transitionDuration)
            .From()
            .SetEase(Ease.OutExpo)
            .SetUpdate(isIndependentUpdate: true);

        return currentTransition = DOTween
            .Sequence()
            .Join(fade)
            .Join(zoom)
            .SetUpdate(isIndependentUpdate: true);
    }

    public void FadeOut(CanvasGroup canvasGroup, Transform transform, bool waitForFadeIn = false)
    {
        if (!waitForFadeIn || currentTransition == null || !currentTransition.IsPlaying())
        {
            currentTransition?.Kill();
            FadeOutInternal(canvasGroup, transform);
            return;
        }

        currentTransition.onComplete += () => FadeOutInternal(canvasGroup, transform);
    }

    private void FadeOutInternal(CanvasGroup canvasGroup, Transform transform)
    {
        canvasGroup.DOKill(complete: true);
        canvasGroup
            .DOFade(0f, transitionDuration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(isIndependentUpdate: true);

        transform.DOKill(complete: true);
        transform
            .DOScale(fadedOutScale, transitionDuration)
            .SetEase(Ease.InExpo)
            .SetUpdate(isIndependentUpdate: true);
    }
}
