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
    [SerializeField] Color colorSuspicious = Color.yellow;
    [SerializeField] Color colorAggressive = Color.red;
    [SerializeField] Image image;
    [Header("Positioning")]
    [SerializeField] Transform trackedTransform;
    [SerializeField] Transform playerTransform;
    [SerializeField] Vector2 interpolationPadding = new Vector2(0.1f, 0.1f);
    
    private Canvas canvas;
    private RectTransform rectTransform;
    
    void Start()
    {
        Assert.IsNotNull(image);
        
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
            return GetPositionForOutOfScreenObject(cameraTransform, innerViewportRect, playerTransform);
        }
        
        // Within the interpolation zone
        if (!innerViewportRect.Contains(viewportPosition))
        {
            var fromCenter = (Vector2)viewportPosition - new Vector2(0.5f, 0.5f);
            var overlap = new Vector2(Mathf.Abs(fromCenter.x), Mathf.Abs(fromCenter.y)) - (new Vector2(0.5f, 0.5f) - interpolationPadding);
            if (overlap.x < 0f) overlap.x = 0f;
            if (overlap.y < 0f) overlap.y = 0f;
            
            float t = Mathf.Clamp01(overlap.x / interpolationPadding.x + overlap.y / interpolationPadding.y);
            Vector3 fromOffscreenPosition = GetPositionForOutOfScreenObject(cameraTransform, innerViewportRect, playerTransform);
            return Vector2.Lerp(viewportPosition, fromOffscreenPosition, t);
        }

        // On screen
        return viewportPosition;
    }

    private Vector3 GetPositionForOutOfScreenObject(Transform cameraTransform, Rect containingRect, Transform targetTransform = null)
    {
        Vector3 relativePos = trackedTransform.position - (targetTransform ? targetTransform : cameraTransform).position;
        Vector2 projectedPos = new Vector2(Vector3.Dot(relativePos, cameraTransform.right), Vector3.Dot(relativePos, cameraTransform.up));
        return Rect.NormalizedToPoint(containingRect, new Vector2(0.5f, 0.5f) + projectedPos.normalized * 0.5f);
    }

    /// stateInterpolation values:
    /// 0: idle
    /// 1: suspicious
    /// 2: aggressive
    /// Values in between interpolate between adjacent states
    /// TEMP Uses simple color interpolation for now. Will have a filling indicator in the future.
    public void SetState(float stateInterpolation)
    {
        stateInterpolation = Mathf.Clamp(stateInterpolation, 0f, 2f);
        if (stateInterpolation <= 1f)
        {
            image.color = Color.Lerp(colorIdle, colorSuspicious, stateInterpolation);
        }
        else
        {
            image.color = Color.Lerp(colorSuspicious, colorAggressive, stateInterpolation - 1f);
        }
    }

    public void SetStateIdle()
    {
        image.color = colorIdle;
    }

    public void SetStateSuspicious()
    {
        image.color = colorSuspicious;
    }

    public void SetStateAggressive()
    {
        image.color = colorAggressive;
    }
}