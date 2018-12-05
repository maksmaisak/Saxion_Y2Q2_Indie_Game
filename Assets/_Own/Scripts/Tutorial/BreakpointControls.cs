using UnityEngine;

public class BreakpointControls : TutorialBreakpoint
{
    protected override bool ReleaseCondition()
    {
        return Input.GetButtonDown("Vertical") || Input.GetButtonDown("Horizontal");
    }
}