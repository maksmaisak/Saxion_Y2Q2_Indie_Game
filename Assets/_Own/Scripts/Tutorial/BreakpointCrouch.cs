using System.Linq;
using UnityEngine;

public class BreakpointCrouch : TutorialBreakpoint
{
    private Animator playerAnimator;
    
    void Start()
    {
        playerAnimator = GameObject
            .FindGameObjectsWithTag("Player")
            .Select(go => go.GetComponentInChildren<Animator>())
            .FirstOrDefault();
    }

    protected override bool DisappearCondition()
    {
        return playerAnimator && playerAnimator.GetBool("Crouch");
    }
}