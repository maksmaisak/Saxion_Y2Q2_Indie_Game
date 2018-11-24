using System;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
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
    [Tooltip("This is used to determine the size of the target object on screen")]
    [SerializeField] Renderer trackedRenderer;
    [SerializeField] Vector2 interpolationPadding = new Vector2(0.1f, 0.1f);

    [Header("Fadeout based on distance from the camera")] 
    [SerializeField] float fadeFullDistance = 20f;
    [SerializeField] float fadeNoneDistance = 50f;
    [Tooltip("Determines how opacity varies as distance goes from fadeFullDistance to fadeNoneDistance.")]
    [SerializeField] AnimationCurve fadeoutCurveDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    
    [Header("Fadeout based on target area in the viewport")] 
    [SerializeField] float fadeFullViewportArea = 0.2f;
    [SerializeField] float fadeNoneViewportArea = 0f;
    [Tooltip("Determines how opacity varies as the object's viewport area goes from fadeFullViewportArea to fadeNoneViewportArea.")]
    [SerializeField] AnimationCurve fadeoutCurveViewportArea = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    private Canvas canvas;
    private RectTransform rectTransform;
    private new Camera camera;
    private Transform cameraTransform;
        
    void Start()
    {
        Assert.IsNotNull(imageIdle);
        Assert.IsNotNull(imageSuspicious);
        Assert.IsNotNull(imageAggressive);
        
        canvas = GetComponentInParent<Canvas>();
        Assert.IsNotNull(canvas);

        rectTransform = GetComponent<RectTransform>();
        Assert.IsNotNull(rectTransform);
        
        camera = Camera.main;
        Assert.IsNotNull(camera);
        cameraTransform = camera.transform;
    }
    
    void LateUpdate()
    {
        if (!trackedTransform) return;

        UpdatePosition();
        UpdateAlpha();
    }

    void OnValidate()
    {
        if (imageIdle) imageIdle.color = colorIdle;
        if (imageSuspicious) imageSuspicious.color = colorSuspicious;
        if (imageAggressive) imageAggressive.color = colorAggressive;
        
        SetState(currentValue);
    }

    private void UpdatePosition()
    {
        rectTransform.anchorMin = rectTransform.anchorMax = GetRectPosition();
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    private void UpdateAlpha()
    {
        float alpha = GetAlpha();
        
        foreach (Image image in new[]{imageIdle, imageSuspicious, imageAggressive})
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
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

    /// Sets the renderer used to determine the size of the tracked object on screen.
    public void SetTrackedRenderer(Renderer newTrackedRenderer)
    {
        trackedRenderer = newTrackedRenderer;
    }
    
    private Vector3 GetRectPosition()
    {
        Vector3 viewportPosition = camera.WorldToViewportPoint(trackedTransform.position);

        var viewportRect = Rect.MinMaxRect(0f, 0f, 1f, 1f);
        var innerViewportRect = viewportRect.Inflated(-interpolationPadding);

        // Off screen
        if (viewportPosition.z < 0f || !viewportRect.Contains(viewportPosition))
        {
            return GetPositionForOutOfScreenObject(innerViewportRect);
        }
        
        // Within the interpolation zone
        if (!innerViewportRect.Contains(viewportPosition))
        {
            float t = GetInterpolationZoneT(viewportPosition);
            Vector3 fromOffscreenPosition = GetPositionForOutOfScreenObject(innerViewportRect);
            return Vector2.Lerp(viewportPosition, fromOffscreenPosition, t);
        }

        // On screen
        return viewportPosition;
    }
        
    private Vector3 GetPositionForOutOfScreenObject(Rect containingRect)
    {
        Vector3 relativePos = trackedTransform.position - cameraTransform.position;
        Vector2 projectedPos = new Vector2(Vector3.Dot(relativePos, cameraTransform.right), Vector3.Dot(relativePos, cameraTransform.up));
        return Rect.NormalizedToPoint(containingRect, new Vector2(0.5f, 0.5f) + projectedPos.normalized * 0.5f);
    }

    private float GetAlpha()
    {
        float distance = (trackedTransform.position - cameraTransform.position).magnitude;
        float distanceBasedAlpha = GetDistanceBasedAlpha(distance);
        
        if (distance < fadeFullDistance) 
            return distanceBasedAlpha;

        return Mathf.Max(distanceBasedAlpha, GetViewportSizeBasedAlpha());
    }

    private float GetDistanceBasedAlpha(float distance)
    {        
        float t;
        if (Mathf.Approximately(fadeNoneDistance, fadeFullDistance))
            t = distance > fadeNoneDistance ? 1f : 0f;
        else
            t = Mathf.InverseLerp(fadeFullDistance, fadeNoneDistance, distance);

        return fadeoutCurveDistance.Evaluate(t);
    }
    
    private float GetViewportSizeBasedAlpha()
    {
        if (!trackedRenderer)
        {
            Debug.LogWarning("No tracked renderer assigned. Can't determine object size on screen. Use SetTrackedRenderer to assign it.");
            return 0f;
        }
        
        // Is it off screen?
        Vector3 viewportPosition = camera.WorldToViewportPoint(trackedTransform.position);
        var viewportRect = Rect.MinMaxRect(0f, 0f, 1f, 1f);
        if (viewportPosition.z < 0f || !viewportRect.Contains(viewportPosition))
        {
            return 0f;
        }

        Rect viewportBounds = trackedRenderer.GetViewportBounds();
        float viewportArea = viewportBounds.width * viewportBounds.height;
        
        float t;
        if (Mathf.Approximately(fadeNoneViewportArea, fadeFullViewportArea))
            t = viewportArea > fadeNoneViewportArea ? 1f : 0f;
        else
            t = Mathf.InverseLerp(fadeFullViewportArea, fadeNoneViewportArea, viewportArea);
        
        float alpha = fadeoutCurveViewportArea.Evaluate(t);
        
        // Fade it out if it's close to the edges of the screen. 
        var interpolationZoneRect = viewportRect.Inflated(-interpolationPadding);
        if (!interpolationZoneRect.Contains(viewportPosition))
        {
            alpha *= 1f - GetInterpolationZoneT(viewportPosition);
        }
        
        return alpha;
    }

    /// 0 is on the inner border, 1 is on the outer border. Assumes the input position is between them.
    private float GetInterpolationZoneT(Vector2 viewportPosition)
    {
        var fromCenter = viewportPosition - new Vector2(0.5f, 0.5f);
        var overlap = new Vector2(Mathf.Abs(fromCenter.x), Mathf.Abs(fromCenter.y)) - (new Vector2(0.5f, 0.5f) - interpolationPadding);
        if (overlap.x < 0f) overlap.x = 0f;
        if (overlap.y < 0f) overlap.y = 0f;
            
        return Mathf.Clamp01(overlap.x / interpolationPadding.x + overlap.y / interpolationPadding.y);
    }
}