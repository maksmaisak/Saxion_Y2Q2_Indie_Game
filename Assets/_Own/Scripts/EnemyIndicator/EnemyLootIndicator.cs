using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class EnemyLootIndicator : Indicator
{
    [SerializeField] CanvasGroup popUpCanvasGroup;
    [SerializeField] TMP_Text popUpCanvasText;
    [SerializeField] GameObject indicator;
    [SerializeField] Image imageLooting;
    [SerializeField] Vector3 offsetFromTransformCenter = new Vector3(0.0f, 1.5f, 0f);
    [SerializeField] float popUpFadeIndDuration = 0.2f;
    [SerializeField] float popUpFadeOutDuration = 0.5f;

    protected override void Start()
    {
       base.Start();
        
        Assert.IsNotNull(indicator);
        Assert.IsNotNull(popUpCanvasGroup);
        Assert.IsNotNull(popUpCanvasText);
    }
    
    public void SetState(float stateInterpolation)
    {
        imageLooting.fillOrigin = (int)Image.OriginVertical.Bottom;
        imageLooting.fillAmount = stateInterpolation;
    }

    public void DoPopUp(int ammo)
    {
        Destroy(indicator);
        
        popUpCanvasGroup.DOFade(1, popUpFadeIndDuration)
            .OnComplete(() => popUpCanvasGroup.DOFade(0, popUpFadeOutDuration)
                .onComplete += () => Destroy(gameObject));
        
        popUpCanvasText.SetText($"+{ammo} ammo");
    }
    
    protected override Vector3 GetTrackedPosition() => trackedRenderer.bounds.center + offsetFromTransformCenter;

}
