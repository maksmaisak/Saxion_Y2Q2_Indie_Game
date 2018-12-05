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
        // TODO: Create a pop up text object instead of setting the text?
        if (message.wasHeadshot)
            deathNotificationDisplayText.text = onHeadshotDeadEnemyText;
        else
            deathNotificationDisplayText.text = onDeadEnemyText;

        OnEnemyDied.Invoke();

        fadeZoom.FadeIn(hitCanvasGroup, hitCanvasGroup.transform);
        fadeZoom.FadeOut(hitCanvasGroup, hitCanvasGroup.transform, true);
    }
}
