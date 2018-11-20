using System;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyIndicatorOnHUD : MonoBehaviour
{
    [SerializeField] Color colorIdle = Color.white;
    [SerializeField] Color colorSuspicious = Color.yellow;
    [SerializeField] Color colorAggressive = Color.red;
    [SerializeField] Image image;
    [SerializeField] Transform trackedTransform;
    [SerializeField] Vector2 viewportPadding;
    //[SerializeField] float maxMagnitude = 0.8f;
    
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
        Debug.Log(viewportPosition);

        if (viewportPosition.z < 0f) viewportPosition = -viewportPosition;
        viewportPosition.x = Mathf.Clamp(viewportPosition.x, viewportPadding.x, 1f - viewportPadding.x);
        viewportPosition.y = Mathf.Clamp(viewportPosition.y, viewportPadding.y, 1f - viewportPadding.y);

        Vector2 viewportPosition2D = new Vector2(viewportPosition.x, viewportPosition.y);
        /*Vector2 fromCenter = viewportPosition2D - new Vector2(0.5f, 0.5f);

        if (fromCenter.magnitude > maxMagnitude)
        {
            viewportPosition2D = new Vector2(0.5f, 0.5f) + fromCenter * maxMagnitude;
        }*/
        
        rectTransform.anchorMin = rectTransform.anchorMax = viewportPosition2D;
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