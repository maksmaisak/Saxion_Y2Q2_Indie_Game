using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BreakpointLoot : TutorialBreakpoint
{
    [SerializeField] EnemyLootable lootable;

    protected override bool DisappearCondition()
    {
        return !lootable || lootable.isLooted;
    }
}