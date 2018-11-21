using System;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class EnemyIndicator : MonoBehaviour
{
    [SerializeField] Color colorIdle = Color.white;
    [SerializeField] Image imageIdle;
    
    [SerializeField] Color colorSuspicious = Color.yellow;
    [SerializeField] Image imageSuspicious;

    [SerializeField] Color colorAggressive = Color.red;
    [SerializeField] Image imageAggressive;

    [SerializeField][Range(0f, 2f)] float currentValue = 0f;

    [Header("Positioning")]
    [SerializeField] Transform trackedTransform;
    [SerializeField] Vector2 interpolationPadding = new Vector2(0.1f, 0.1f);
    
    private Canvas canvas;
    private RectTransform rectTransform;
    
    void Start()
    {
        Assert.IsNotNull(imageIdle);
        Assert.IsNotNull(imageSuspicious);
        Assert.IsNotNull(imageAggressive);
        
        canvas = GetComponentInParent<Canvas>();
        Assert.IsNotNull(canvas);

        rectTransform = GetComponent<RectTransform>();
        Assert.IsNotNull(rectTransform);
    }
    
    void LateUpdate()
    {
        if (!trackedTransform) return;

        rectTransform.anchorMin = rectTransform.anchorMax = GetRectPosition();
        rectTransform.anchoredPosition = Vector2.zero;
    }

    void OnValidate()
    {
        if (imageIdle) imageIdle.color = colorIdle;
        if (imageSuspicious) imageSuspicious.color = colorSuspicious;
        if (imageAggressive) imageAggressive.color = colorAggressive;
        
        SetState(currentValue);
    }

    /// stateInterpolation values:
    /// 0: idle
    /// 1: suspicious
    /// 2: aggressive
    /// Values in between interpolate between adjacent states
    public void SetState(float stateInterpolation)
    {
        currentValue = stateInterpolation;

        stateInterpolation = Mathf.Clamp(stateInterpolation, 0f, 2f);
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

    public void SetTrackedTransform(Transform newTrackedTransform)
    {
        trackedTransform = newTrackedTransform;
    }
    
    private Vector3 GetRectPosition()
    {
        Camera camera = Camera.main;
        Transform cameraTransform = camera.transform;
        Vector3 viewportPosition = camera.WorldToViewportPoint(trackedTransform.position);

        var viewportRect = Rect.MinMaxRect(0f, 0f, 1f, 1f);
        var innerViewportRect = viewportRect.Inflated(-interpolationPadding);

        // Off screen
        if (viewportPosition.z < 0f || !viewportRect.Contains(viewportPosition))
        {
            return GetPositionForOutOfScreenObject(cameraTransform, innerViewportRect);
        }
        
        // Within the interpolation zone
        if (!innerViewportRect.Contains(viewportPosition))
        {
            var fromCenter = (Vector2)viewportPosition - new Vector2(0.5f, 0.5f);
            var overlap = new Vector2(Mathf.Abs(fromCenter.x), Mathf.Abs(fromCenter.y)) - (new Vector2(0.5f, 0.5f) - interpolationPadding);
            if (overlap.x < 0f) overlap.x = 0f;
            if (overlap.y < 0f) overlap.y = 0f;
            
            float t = Mathf.Clamp01(overlap.x / interpolationPadding.x + overlap.y / interpolationPadding.y);
            Vector3 fromOffscreenPosition = GetPositionForOutOfScreenObject(cameraTransform, innerViewportRect);
            return Vector2.Lerp(viewportPosition, fromOffscreenPosition, t);
        }

        // On screen
        return viewportPosition;
    }

    private Vector3 GetPositionForOutOfScreenObject(Transform cameraTransform, Rect containingRect)
    {
        Vector3 relativePos = trackedTransform.position - cameraTransform.position;
        Vector2 projectedPos = new Vector2(Vector3.Dot(relativePos, cameraTransform.right), Vector3.Dot(relativePos, cameraTransform.up));
        return Rect.NormalizedToPoint(containingRect, new Vector2(0.5f, 0.5f) + projectedPos.normalized * 0.5f);
    }
}