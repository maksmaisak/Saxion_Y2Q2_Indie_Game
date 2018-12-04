using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public abstract class Indicator : MonoBehaviour
{ 
    struct RectPositionAndRotation
    {
        public Vector2 position;
        public Quaternion rotation;

        public RectPositionAndRotation(Vector2 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
    
    [Header("Positioning")]
    [Tooltip("This is used to determine the size of the target object on screen")]
    [SerializeField] protected Renderer trackedRenderer;
    [SerializeField] protected Vector2 interpolationPadding = new Vector2(0.1f, 0.1f);
    
    [Header("Fadeout based on distance from the camera")] 
    [SerializeField] protected float fadeFullDistance = 20f;
    [SerializeField] protected float fadeNoneDistance = 50f;
    [Tooltip("Determines how opacity varies as distance goes from fadeFullDistance to fadeNoneDistance.")]
    [SerializeField] protected AnimationCurve fadeoutCurveDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    
    [Header("Fadeout based on target area in the viewport")] 
    [SerializeField] protected float fadeFullViewportArea = 0.2f;
    [SerializeField] protected float fadeNoneViewportArea = 0f;
    [Tooltip("Determines how opacity varies as the object's viewport area goes from fadeFullViewportArea to fadeNoneViewportArea.")]
    [SerializeField] protected AnimationCurve fadeoutCurveViewportArea = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    
    private Canvas canvas;
    private RectTransform rectTransform;
    private new Camera camera;
    private Transform cameraTransform;

    private List<Image> images = new List<Image>();
    
    protected Vector3 trackedPosition = Vector3.zero;

    protected virtual void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        Assert.IsNotNull(canvas);

        rectTransform = GetComponent<RectTransform>();
        Assert.IsNotNull(rectTransform);
        
        camera = Camera.main;
        Assert.IsNotNull(camera);
        cameraTransform = camera.transform;

        foreach (Image image in GetComponentsInChildren<Image>())
            images.Add(image);
    }

    protected abstract Vector3 GetTrackedPosition();
    
    /// Sets the renderer used to determine the size of the tracked object on screen.
    public void SetTrackedRenderer(Renderer newTrackedRenderer)
    {
        trackedRenderer = newTrackedRenderer;
    }

    private void LateUpdate()
    {        
        trackedPosition = GetTrackedPosition();
        
        UpdatePositionAndRotation();
        UpdateAlpha();
    }
    
    private void UpdatePositionAndRotation()
    {
        RectPositionAndRotation config = GetRectPositionAndRotation();
        rectTransform.rotation = config.rotation;
        rectTransform.anchorMin = rectTransform.anchorMax = config.position;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    private RectPositionAndRotation GetRectPositionAndRotation()
    {
        Vector3 viewportPosition = camera.WorldToViewportPoint(trackedPosition);

        var viewportRect = Rect.MinMaxRect(0f, 0f, 1f, 1f);
        var innerViewportRect = viewportRect.Inflated(-interpolationPadding);

        // Off screen
        if (viewportPosition.z < 0f || !viewportRect.Contains(viewportPosition))
        {
            return GetPositionAndRotationForOutOfScreenObject(innerViewportRect);
        }
        
        // Within the interpolation zone
        if (!innerViewportRect.Contains(viewportPosition))
        {
            float t = GetInterpolationZoneT(viewportPosition);
            RectPositionAndRotation fromOffScreen = GetPositionAndRotationForOutOfScreenObject(innerViewportRect);
            return new RectPositionAndRotation(
                Vector2.Lerp(viewportPosition, fromOffScreen.position, t),
                Quaternion.Slerp(Quaternion.identity, fromOffScreen.rotation, t)
            );
        }

        // On screen
        return new RectPositionAndRotation(viewportPosition, Quaternion.identity);
    }
    
    private RectPositionAndRotation GetPositionAndRotationForOutOfScreenObject(Rect containingRect)
    {
        Vector3 relativePos = trackedPosition - cameraTransform.position;
        Vector2 projectedPos = new Vector2(Vector3.Dot(relativePos, cameraTransform.right), Vector3.Dot(relativePos, cameraTransform.up));
        Vector2 direction = projectedPos.normalized;
        
        Vector2 viewportPosition = Rect.NormalizedToPoint(containingRect, new Vector2(0.5f, 0.5f) + direction * 0.5f);
        var rotation = Quaternion.Euler(Vector3.forward * (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f));
        return new RectPositionAndRotation(viewportPosition, rotation);
    }

    private void UpdateAlpha()
    {
        float alpha = GetAlpha();

        foreach (Image image in images)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    private float GetAlpha()
    {
        float distance = (trackedPosition - cameraTransform.position).magnitude;
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
        Vector3 viewportPosition = camera.WorldToViewportPoint(trackedPosition);
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