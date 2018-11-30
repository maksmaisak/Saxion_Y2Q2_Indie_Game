using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class LevelEndScreen : MyBehaviour, IEventReceiver<LevelFinished>
{
    [SerializeField] CanvasGroup main;
    [SerializeField] float mainFadeInDelay = 0f;
    [SerializeField] float mainFadeInTime = 1f;
    [Space] 
    [SerializeField] private float statsFadeInDelay = 1f;
    [Space]
    [SerializeField] CanvasGroup buttons;
    [SerializeField] float buttonFadeInDelay = 3f;
    [SerializeField] float buttonsFadeInTime = 2f;
        
    void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        this.Delay(2f, () => new LevelFinished().PostEvent());
    }

    public void On(LevelFinished message)
    {
        if (!isActiveAndEnabled) return;
				
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        if (main)
        {
            main.alpha = 0f;
            main
                .DOFade(1f, mainFadeInTime)
                .SetDelay(mainFadeInDelay);
            main.interactable = main.blocksRaycasts = true;
        }
        
        Sequence statsSequence = DOTween.Sequence().AppendInterval(statsFadeInDelay);
        foreach (StatsLine statsLine in GetComponentsInChildren<StatsLine>())
        {
            statsSequence.Append(statsLine.TransitionIn());
        }

        if (buttons)
        {
            buttons.alpha = 0f;
            buttons.interactable = false;
            buttons.blocksRaycasts = false;

            DOTween.Sequence()
                .AppendInterval(buttonFadeInDelay)
                .AppendCallback(() =>
                {
                    CursorHelper.SetLock(false);
                    buttons.interactable = true;
                    buttons.blocksRaycasts = true;
                    buttons.GetComponentInChildren<Button>()?.Select();
                })
                .Append(
                    buttons
                        .DOFade(1f, buttonsFadeInTime)
                        .SetEase(Ease.InExpo)
                );
        }
    }
}