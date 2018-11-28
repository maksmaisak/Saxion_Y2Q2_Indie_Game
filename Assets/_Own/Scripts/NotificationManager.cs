using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;

public class NotificationManager : MyBehaviour, IEventReceiver<EnemyDeath>
{
    [SerializeField] string onHeadshotDeadEnemyText = "Enemy killed (Headshot)";
    [SerializeField] string onDeadEnemyText = "Enemy killed.";
    [SerializeField] TMP_Text deathNotificationDisplayText;
    [SerializeField] CanvasGroup hitCanvasGroup;
    [SerializeField] FadeZoom fadeZoom;

    private void Start()
    {
        Debug.Assert(deathNotificationDisplayText);
        Debug.Assert(hitCanvasGroup);
    }

    public void On(EnemyDeath message)
    {
        // TODO: Create a pop up text object instead of setting the text?
        if (message.wasHeadshot)
            deathNotificationDisplayText.text = onHeadshotDeadEnemyText;
        else
            deathNotificationDisplayText.text = onDeadEnemyText;
        
        fadeZoom.FadeIn(hitCanvasGroup, hitCanvasGroup.transform);
        fadeZoom.FadeOut(hitCanvasGroup, hitCanvasGroup.transform, true);
    }
}
