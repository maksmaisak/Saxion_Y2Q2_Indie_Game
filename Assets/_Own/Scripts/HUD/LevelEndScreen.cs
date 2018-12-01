using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class LevelEndScreen : MyBehaviour, IEventReceiver<LevelFinished>
{
    [SerializeField] CanvasGroup main;
    [SerializeField] float mainFadeInDelay = 0f;
    [SerializeField] float mainFadeInTime = 1f;
    [Space] 
    [SerializeField] StatsLine lineScore;
    [SerializeField] StatsLine lineKills;
    [SerializeField] StatsLine lineHeadshots;
    [SerializeField] float statsFadeInDelay = 1f;
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
        
        Assert.IsTrue(StatsTracker.exists, "No StatsTracker found. Add it to the __app gameobject on the __preload scene.");
        var stats = StatsTracker.instance;
        
        Sequence statsSequence = DOTween.Sequence().AppendInterval(statsFadeInDelay);
        if (lineScore    ) statsSequence.Append(lineScore    .TransitionIn(stats.score       ));
        if (lineKills    ) statsSequence.Append(lineKills    .TransitionIn(stats.numKills    ));
        if (lineHeadshots) statsSequence.Append(lineHeadshots.TransitionIn(stats.numHeadshots));
        
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