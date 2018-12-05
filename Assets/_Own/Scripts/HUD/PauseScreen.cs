using UnityEngine;

public class PauseScreen : FadeZoomScreen, 
    IEventReceiver<OnPlayerDied>,
    IEventReceiver<OnLevelCompleted>
{
    private float initialTimeScale = 1f;
    
    void Update()
    {
        if (!Input.GetButtonDown("Pause")) return;
        
        if (isCurrentlySelected) TransitionOut();
        else TransitionIn();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (isCurrentlySelected) 
            Time.timeScale = initialTimeScale;
    }

    public void On(OnPlayerDied message) => MakeSureItsUnpaused();
    public void On(OnLevelCompleted message) => MakeSureItsUnpaused();

    private void MakeSureItsUnpaused()
    {
        TransitionOut();
        gameObject.SetActive(false);
    }

    protected override void OnTransitionIn()
    {
        base.OnTransitionIn();
        
        initialTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        CursorHelper.SetLock(false);
    }

    protected override void OnTransitionOut()
    {
        base.OnTransitionOut();

        Time.timeScale = initialTimeScale;
        
        CursorHelper.SetLock(true);
    }
}