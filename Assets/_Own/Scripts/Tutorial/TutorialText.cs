using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

// A wrapper around a textmeshpro text hovering over things in the tutorial. 
public class TutorialText : MonoBehaviour
{
    [SerializeField] TMP_Text textMesh;
    [SerializeField] float appearDuration = 0.5f;
    [SerializeField] float disappearDuration = 0.5f;
    [SerializeField] bool hideOnStart = true;

    private string fullText;

    void Awake()
    {
        if (!textMesh) textMesh = GetComponentInChildren<TMP_Text>();
        if (!textMesh) return;
        
        fullText = textMesh.text;
        if (hideOnStart) textMesh.enabled = false;
    }
    
    public void Appear()
    {
        if (!textMesh) return;
        
        textMesh.DOKill();
        textMesh.text = string.Empty;
        textMesh.enabled = true;
        textMesh.alpha = 1f;
        textMesh
            .DOText(fullText, appearDuration)
            .SetUpdate(isIndependentUpdate: true);
    }

    public void Disappear()
    {
        if (!textMesh) return;
        
        textMesh
            .DOFade(0f, disappearDuration)
            .SetEase(Ease.InExpo);
    }
}