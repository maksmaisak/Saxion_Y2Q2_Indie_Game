using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class BreakpointLoot : TutorialBreakpoint
{
    [SerializeField] TMP_Text textMesh;
    [SerializeField] EnemyLootable lootable;
    [SerializeField] float transitionDuration = 1f;

    private string fullText;

    protected override void Start()
    {
        base.Start();
        Assert.IsNotNull(textMesh);
        fullText = textMesh.text;
    }

    protected override void OnActivate()
    {
        textMesh.enabled = true;
        textMesh.text = string.Empty;
        textMesh.DOText(fullText, transitionDuration);
    }

    protected override void OnDeactivate()
    {
        textMesh.enabled = false;
    }

    protected override bool DisappearCondition()
    {
        return !lootable || lootable.isLooted;
    }
}