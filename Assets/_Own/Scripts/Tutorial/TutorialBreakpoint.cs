using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;

public abstract class TutorialBreakpoint : MonoBehaviour
{
    [SerializeField] float appearDelay = 0f;
    [SerializeField] float disappearDelay = 0f;
    [SerializeField] float timescaleWhenActive = 1f;
    [SerializeField] bool dontStartIfConditionMetEarly;

    [SerializeField] UnityEvent OnTrigger;
    [SerializeField] UnityEvent OnAppear;
    [SerializeField] UnityEvent OnConditionSatisfied;
    [SerializeField] UnityEvent OnDisappear;

    protected bool wasTriggered { get; private set; }
    protected bool isActive { get; private set; }

    protected virtual void Update()
    {
        if (!wasTriggered)
        {
            if (dontStartIfConditionMetEarly && DisappearCondition())
            {
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
        this.Delay(appearDelay, () =>
        {
            OnAppear.Invoke();
            OnActivate();
            GetComponent<TutorialText>()?.Appear();
        });
    }

    public void Disappear()
    {
        // Make it a sequence instead of with Delay because coroutines are stopped OnDisable
        DOTween.Sequence()
            .AppendInterval(disappearDelay)
            .AppendCallback(() =>
            {
                OnDisappear.Invoke();
                OnDeactivate();
                GetComponent<TutorialText>()?.Disappear();
            });

        TimeHelper.timeScale = 1f;
        isActive = false;
        enabled = false;
    }
}