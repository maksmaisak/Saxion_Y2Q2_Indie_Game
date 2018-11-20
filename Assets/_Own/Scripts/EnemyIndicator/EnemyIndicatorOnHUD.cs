using System;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class EnemyIndicatorOnHUD : MonoBehaviour
{
    [SerializeField] Color colorIdle = Color.white;
    [SerializeField] Color colorSuspicious = Color.yellow;
    [SerializeField] Color colorAggressive = Color.red;
    [SerializeField] Image image;
    [SerializeField] Transform trackedTransform;
    [SerializeField] Vector2 viewportPadding;
    
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

        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(trackedTransform.position);

        if (viewportPosition.z < 0f) viewportPosition = -viewportPosition;
        viewportPosition.x = Mathf.Clamp(viewportPosition.x, viewportPadding.x, 1f - viewportPadding.x);
        viewportPosition.y = Mathf.Clamp(viewportPosition.y, viewportPadding.y, 1f - viewportPadding.y);
        
        rectTransform.anchorMin = rectTransform.anchorMax = viewportPosition;
        rectTransform.anchoredPosition = Vector2.zero;
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