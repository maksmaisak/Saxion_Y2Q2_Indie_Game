using DG.Tweening;
using TMPro;
using UnityEngine;

public static class TextTween
{
    public static Tweener DOText(this TMP_Text textMesh, string finalText, float duration)
    {
        float t = 0f;
        return DOTween.To(
            getter: () => t,
            setter: newT =>
            {
                t = newT;
                textMesh.text = finalText.Substring(0, Mathf.FloorToInt(finalText.Length * t));
            },
            endValue: 1f,
            duration: duration
        )
        .SetTarget(textMesh)
        .SetEase(Ease.Linear);
    }
}