using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class StatsLine : MonoBehaviour 
{    
    [SerializeField] FadeZoom fadeZoom;
    [SerializeField] TMP_Text valueText;
    [SerializeField] float textTransitionDuration = 1f;
    
    public int finalValue { get; set; } = 100;
    
    public Tween TransitionIn()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        var sequence = DOTween.Sequence();
        
        var fade = fadeZoom.FadeIn(canvasGroup, transform);
        sequence.Append(fade);

        if (valueText)
        {
            float currentValue = 0f;
            valueText.SetText("0");
            var text = DOTween.To(
                () => (int)currentValue,
                (newValue) =>
                {
                    currentValue = newValue;
                    valueText.SetText(Mathf.RoundToInt(currentValue).ToString());
                },
                (float)finalValue,
                textTransitionDuration
            );

            sequence.Append(text);
        }

        return sequence;
    }
}