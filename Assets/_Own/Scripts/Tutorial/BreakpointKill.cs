using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class BreakpointKill : TutorialBreakpoint
{
    [SerializeField] Health target;
    
    private bool didTargetDie;
    
    protected override void Start()
    {
        base.Start();
        
        Assert.IsNotNull(target);
        target.OnDeath += sender => didTargetDie = true;
    }

    protected override bool DisappearCondition() => didTargetDie;
}