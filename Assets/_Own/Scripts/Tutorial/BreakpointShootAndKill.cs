using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class BreakpointShootAndKill : TutorialBreakpoint
{
    [SerializeField] Health target;

    [SerializeField] TutorialText[] visibleWhenNotSniping;
    [SerializeField] TutorialText[] visibleWhenSniping;
    [SerializeField] TutorialText[] visibleWhileActive;
    
    private PlayerCameraController playerCameraController;
    private bool didTargetDie;
    
    void Start()
    {        
        playerCameraController = FindObjectOfType<PlayerCameraController>();

        Assert.IsNotNull(target);
        target.OnDeath += sender => didTargetDie = true;
    }

    protected override void Update()
    {
        base.Update();
        if (!enabled) return;
        
        if (playerCameraController.isSniping)
        {
            foreach (var tutorialText in visibleWhenSniping) tutorialText.Appear();
            foreach (var tutorialText in visibleWhenNotSniping) tutorialText.Disappear();
        }
        else
        {
            foreach (var tutorialText in visibleWhenSniping) tutorialText.Disappear();
            foreach (var tutorialText in visibleWhenNotSniping) tutorialText.Appear();
        }
    }
    
    protected override void OnActivate()
    {
        foreach (var tutorialText in visibleWhileActive) tutorialText.Appear();
    }

    protected override void OnDeactivate()
    {
        foreach (var tutorialText in visibleWhileActive) tutorialText.Disappear();
        foreach (var tutorialText in visibleWhenSniping) tutorialText.Disappear();
        foreach (var tutorialText in visibleWhenNotSniping) tutorialText.Disappear();
    }

    protected override bool DisappearCondition() => didTargetDie;
}