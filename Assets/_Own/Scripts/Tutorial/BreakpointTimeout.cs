using UnityEngine;

public class BreakpointTimeout : TutorialBreakpoint
{
    [SerializeField] float duration = 2f;
    
    private float timeOfActivation;
    
    protected override void OnActivate()
    {
        timeOfActivation = Time.unscaledTime;
    }

    protected override bool ReleaseCondition()
    {
        return wasTriggered && Time.unscaledTime - timeOfActivation > duration;
    }
}