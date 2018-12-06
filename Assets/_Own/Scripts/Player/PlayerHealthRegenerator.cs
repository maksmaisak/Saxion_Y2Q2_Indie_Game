using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Health))]
public class PlayerHealthRegenerator : MyBehaviour, IEventReceiver<OnEnemyCombat>
{
    [Tooltip("Regenerate health every X seconds.")]
    [SerializeField] float healthRegenerationPeriod = 1.0f;
    [SerializeField] float outOfCombatTimeToHealthRegeneration = 10.0f;
    [SerializeField] int healthRegenerationAmount = 5;
    [Tooltip("Enable this to use the amount as a percent instead of raw health.")]
    [SerializeField] bool regeneratePercent = false;
    [SerializeField] UnityEvent OnEnterCombat;
    [SerializeField] UnityEvent OnExitCombat;

    private Health health;

    public void On(OnEnemyCombat combat)
    {
        // If the enemy enters combat then don't start a new regeneration timer and stop the current one
        if (combat.enterCombat)
        {
            OnEnterCombat.Invoke();
            StopAllCoroutines();
            return;
        }
        
        // Start a delay and check if the player is finally out of combat
        this.Delay(0.05f, () =>
        {
            // There are still agents in combat so don't do anything yet.
            if (AIManager.instance.GetAllAgentsInCombat().Count > 0)
                return;

            OnExitCombat.Invoke();
            
            // Start health regeneration out of combat
            StartCoroutine(RegenerateHealthCoroutine());
        });
    }

    private void Start() => health = GetComponent<Health>();
    private void OnDisable() => StopAllCoroutines();

    private IEnumerator RegenerateHealthCoroutine()
    {
        yield return new WaitForSeconds(outOfCombatTimeToHealthRegeneration);

        int increaseAmount = regeneratePercent ? (int)(health.maxHealth * (float)healthRegenerationAmount / 100.0f) : healthRegenerationAmount;

        while(true)
        {
            health.Increase(increaseAmount);
            yield return new WaitForSeconds(healthRegenerationPeriod);
        }
    }
}
