using UnityEngine;
using UnityEngine.UI;

public class EnemyLootIndicator : Indicator
{
    [SerializeField] Image imageUnlooted;
    [SerializeField] Image imageLooting;
    [SerializeField] Vector3 offsetFromTransformCenter = new Vector3(0.0f, 1.5f, 0f);
    
    public void SetState(float stateInterpolation)
    {        
        imageUnlooted.fillOrigin = (int)Image.OriginVertical.Top;
        imageUnlooted.fillAmount = 1f - stateInterpolation;
        imageLooting.fillOrigin = (int)Image.OriginVertical.Bottom;
        imageLooting.fillAmount = stateInterpolation;
    }

    protected override Vector3 GetTrackedPosition() => trackedRenderer.bounds.center + offsetFromTransformCenter;

}
