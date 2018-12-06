using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NotificationManager : MyBehaviour, IEventReceiver<OnEnemyDied>
{
    [SerializeField] string onHeadshotDeadEnemyText = "MUTANT KILLED";
    [SerializeField] string onDeadEnemyText = "HEADSHOT";
    [SerializeField] TMP_Text deathNotificationDisplayText;
    [SerializeField] CanvasGroup hitCanvasGroup;
    [SerializeField] FadeZoom fadeZoom;
    [SerializeField] UnityEvent OnEnemyDied;

    private void Start()
    {
        Debug.Assert(deathNotificationDisplayText);
        Debug.Assert(hitCanvasGroup);
    }

    public void On(OnEnemyDied message)
    {
        int scoreBonus = StatsTracker.instance.getScoreBonuses.GetScoreFor(message);

        if (message.wasHeadshot)
            deathNotificationDisplayText.text = onHeadshotDeadEnemyText + $" (+{scoreBonus})";
        else
            deathNotificationDisplayText.text = onDeadEnemyText + $" (+{scoreBonus})";

        OnEnemyDied.Invoke();

        fadeZoom.FadeIn(hitCanvasGroup, hitCanvasGroup.transform);
        fadeZoom.FadeOut(hitCanvasGroup, hitCanvasGroup.transform, true);
    }
}
