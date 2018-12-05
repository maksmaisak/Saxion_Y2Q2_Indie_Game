using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class TutorialBreakpoint : MonoBehaviour
{
    [SerializeField] Transform uiTransform;
    [SerializeField] float appearDuration = 0.4f;
    [SerializeField] float disappearDuration = 0.4f;
    [SerializeField] float timescaleWhenSlow = 0.01f;
    [SerializeField] bool dontStartIfConditionMetEarly;

    private Tween appearTween;
    protected bool wasTriggered { get; private set; }

    void Start()
    {
        Assert.IsNotNull(uiTransform);
        appearTween = uiTransform
            .DOScale(Vector3.zero, appearDuration)
            .From()
            .SetEase(Ease.InExpo)
            .SetUpdate(isIndependentUpdate: true)
            .Pause();
    }

    void Update()
    {
        if (!wasTriggered)
        {
            if (dontStartIfConditionMetEarly && ReleaseCondition())
            {
                uiTransform.DOKill();
                enabled = false;
                return;
            }
        }
        
        if (ReleaseCondition()) Release();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        if (!other.CompareTag("Player")) return;

        wasTriggered = true;

        TimeHelper.timeScale = timescaleWhenSlow;
        appearTween.Play();
        
        OnActivate();
    }

    protected abstract bool ReleaseCondition();
    protected virtual void OnActivate() {}

    private void Release()
    {
        uiTransform.DOKill();
        uiTransform
            .DOScale(Vector3.zero, disappearDuration)
            .SetEase(Ease.InExpo)
            .SetUpdate(isIndependentUpdate: true)
            .Pause();
        
        TimeHelper.timeScale = 1f;
        enabled = false;
    }
}
