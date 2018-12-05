using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;

public abstract class TutorialBreakpoint : MonoBehaviour
{
    [SerializeField] Transform uiTransform;
    [SerializeField] float appearDuration = 0.4f;
    [SerializeField] float appearDelay = 0f;
    [SerializeField] float disappearDuration = 0.4f;
    [SerializeField] float disappearDelay = 0f;
    [SerializeField] float timescaleWhenActive = 1f;
    [SerializeField] bool dontStartIfConditionMetEarly;

    [SerializeField] UnityEvent OnTrigger;
    [SerializeField] UnityEvent OnAppear;
    [SerializeField] UnityEvent OnConditionSatisfied;
    [SerializeField] UnityEvent OnDisappear;

    private Tween appearTween;
    protected bool wasTriggered { get; private set; }
    protected bool isActive { get; private set; }

    protected virtual void Start()
    {        
        var sequence = DOTween
            .Sequence()
            .AppendInterval(appearDelay)
            .AppendCallback(OnAppear.Invoke);

        if (uiTransform)
        {
            sequence.Append(
                uiTransform
                    .DOScale(Vector3.zero, appearDuration)
                    .From()
                    .SetEase(Ease.InExpo)
                    .SetUpdate(isIndependentUpdate: true)
            );
        }

        sequence.Pause();
        appearTween = sequence;
    }

    protected virtual void Update()
    {
        if (!wasTriggered)
        {
            if (dontStartIfConditionMetEarly && DisappearCondition())
            {
                uiTransform.DOKill();
                enabled = false;
                return;
            }

            if (AppearCondition())
            {
                Appear();
            }
        }

        if (DisappearCondition())
        {
            OnConditionSatisfied.Invoke();
            Disappear();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enabled && other.CompareTag("Player"))
        {
            Appear();
        }
    }

    protected virtual bool AppearCondition() => false;
    protected virtual bool DisappearCondition() => false;
    protected virtual void OnActivate() {}
    protected virtual void OnDeactivate() {}

    public void Appear()
    {
        wasTriggered = true;
        isActive = true;
        TimeHelper.timeScale = timescaleWhenActive;
        
        OnTrigger.Invoke();
        appearTween.Play();

        OnActivate();
    }

    public void Disappear()
    {
        uiTransform.DOKill();
        uiTransform
            .DOScale(Vector3.zero, disappearDuration)
            .SetEase(Ease.InExpo)
            .SetUpdate(isIndependentUpdate: true)
            .SetDelay(disappearDelay)
            .OnStart(OnDisappear.Invoke);

        TimeHelper.timeScale = 1f;
        isActive = false;
        enabled = false;
    }
}