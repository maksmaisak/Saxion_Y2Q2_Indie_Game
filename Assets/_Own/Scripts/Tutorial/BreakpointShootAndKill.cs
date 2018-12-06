using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class BreakpointShootAndKill : TutorialBreakpoint
{
    [SerializeField] Health target;

    [SerializeField] TMP_Text textMeshAim;
    [SerializeField] TMP_Text textMeshShoot;
    [SerializeField] float textAppearDuration = 1f;

    private PlayerCameraController playerCameraController;
    private string textAim;
    private string textShoot;
    private bool didTargetDie;
    
    void Start()
    {        
        playerCameraController = FindObjectOfType<PlayerCameraController>();

        if (textMeshAim)
        {
            textAim = textMeshAim.text;
            textMeshAim.enabled = false;
        }

        if (textMeshShoot)
        {
            textShoot = textMeshShoot.text;
            textMeshShoot.enabled = false;
        }

        Assert.IsNotNull(target);
        target.OnDeath += sender => didTargetDie = true;
    }

    protected override void Update()
    {
        base.Update();
        if (!isActive)
        {
            if (textMeshAim) textMeshAim.enabled = false;
            if (textMeshShoot) textMeshShoot.enabled = false;
            return;
        }

        if (!playerCameraController.isSniping && textMeshAim)
        {
            if (textMeshShoot) textMeshShoot.enabled = false;
            
            if (textMeshAim.enabled) return;
            textMeshAim.enabled = true;
            textMeshAim.text = string.Empty;
            textMeshAim.DOKill();
            textMeshAim.DOText(textAim, textAppearDuration);
        }
        else
        {
            if (textMeshAim) textMeshAim.enabled = false;
            
            if (!textMeshShoot || textMeshShoot.enabled) return;
            textMeshShoot.enabled = true;
            textMeshShoot.text = string.Empty;
            textMeshShoot.DOKill();
            textMeshShoot.DOText(textShoot, textAppearDuration);
        }
    }

    protected override void OnDeactivate()
    {
        gameObject.SetActive(false);
    }

    protected override bool DisappearCondition() => didTargetDie;
}