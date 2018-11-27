using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerDamageFeedback : MonoBehaviour
{
    private DamageOverlay damageOverlay;
    
    void Start()
    {
        damageOverlay = LevelCanvas.Get().damageOverlay;
        GetComponent<Health>().OnHealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged(Health sender, int oldValue, int newValue)
    {
        if (newValue >= oldValue) return;
        
        damageOverlay.ShowDamage(oldValue - newValue);
    }
}