using UnityEngine;

public class BreakpointWASD : TutorialBreakpoint
{
    protected override bool DisappearCondition()
    {
        return Input.GetButtonDown("Vertical") || Input.GetButtonDown("Horizontal");
    }
}