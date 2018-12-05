using System;
using Cinemachine.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[ExecuteInEditMode]
public class EnemyIndicator : Indicator
{
    [SerializeField] Color colorIdle = Color.white;
    [SerializeField] Image imageIdle;
    
    [SerializeField] Color colorSuspicious = Color.yellow;
    [SerializeField] Image imageSuspicious;

    [SerializeField] Color colorAggressive = Color.red;
    [SerializeField] Image imageAggressive;

    [SerializeField][Range(0f, 2f)] float currentValue = 0f;

    [SerializeField] AlertSettings heardSomethingAlertSettings;
    [SerializeField] AlertSettings detectedPlayerAlertSettings;

    [Serializable]
    class AlertSettings
    {
        public Vector3 maxScale = new Vector3(1.5f, 1.5f, 1.5f);
        public float duration = 0.2f;
        public int vibrato = 1;
        public float elasticity = 10f;
    }

    private Transform trackedTransform;
    
    protected override void Start()
    {
        Assert.IsNotNull(imageIdle);
        Assert.IsNotNull(imageSuspicious);
        Assert.IsNotNull(imageAggressive);
        
        base.Start();
    }

    void OnValidate()
    {
        if (imageIdle) imageIdle.color = colorIdle;
        if (imageSuspicious) imageSuspicious.color = colorSuspicious;
        if (imageAggressive) imageAggressive.color = colorAggressive;
        
        SetState(currentValue);
    }

    public void SetTrackedTransform(Transform trackedTransform)
    {
        this.trackedTransform = trackedTransform;
    }

    protected override Vector3 GetTrackedPosition() => trackedTransform.position;
    
    /// stateInterpolation values:
    /// 0: idle
    /// 1: suspicious
    /// 2: aggressive
    /// Values in between interpolate between adjacent states
    public void SetState(float stateInterpolation)
    {
        currentValue = stateInterpolation = Mathf.Clamp(stateInterpolation, 0f, 2f);
        
        if (stateInterpolation <= 1f)
        {
            imageIdle.fillOrigin = (int)Image.OriginVertical.Top;
            imageIdle.fillAmount = 1f - stateInterpolation;
            imageSuspicious.fillOrigin = (int)Image.OriginVertical.Bottom;
            imageSuspicious.fillAmount = stateInterpolation;

            imageAggressive.fillAmount = 0f;
        }
        else
        {
            stateInterpolation -= 1f;

            imageSuspicious.fillOrigin = (int)Image.OriginVertical.Top;
            imageSuspicious.fillAmount = 1f - stateInterpolation;
            imageAggressive.fillOrigin = (int)Image.OriginVertical.Bottom;
            imageAggressive.fillAmount = stateInterpolation;

            imageIdle.fillAmount = 0f;
        }
    }

    public void ShowAlertHeardSomething()
    {
        transform.DOKill(complete: true);
        transform.DOPunchScale(
            heardSomethingAlertSettings.maxScale, 
            heardSomethingAlertSettings.duration, 
            heardSomethingAlertSettings.vibrato,
            heardSomethingAlertSettings.elasticity
        );
    }

    public void ShowAlertDetectedPlayer()
    {
        transform.DOKill(complete: true);
        transform.DOPunchScale(
            detectedPlayerAlertSettings.maxScale, 
            detectedPlayerAlertSettings.duration, 
            detectedPlayerAlertSettings.vibrato,
            detectedPlayerAlertSettings.elasticity
        );
    }
}