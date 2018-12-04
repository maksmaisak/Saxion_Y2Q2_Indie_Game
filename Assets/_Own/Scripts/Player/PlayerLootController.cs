using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerLootController : MonoBehaviour
{
    [SerializeField] float lootDistance = 2f;
    [Tooltip("Set the angle at which the player can see the lootable objects")]
    [SerializeField] float lootMaxAngle = 45.0f;
    [SerializeField] LayerMask blockingLayerMask;
    [SerializeField] string lootActionButtonName = "Loot";

    private EnemyAI currentLootAgent = null;
    private EnemyLootIndicator currentLootIndicator = null;

    private const float rememberLootingProgressInSeconds = 0.6f;
    private float lootingProgress;
    
    private bool isLooting;
    
    private void Update()
    {
        UpdateLootInput();
        UpdateLootState();
    }

    private void UpdateLootInput()
    {        
        if (Input.GetButtonDown(lootActionButtonName))
        {            
            currentLootAgent  = SelectLootTarget();

            if (currentLootAgent != null)
            {                
                lootingProgress       = 0;
                isLooting             = true;
                currentLootIndicator  = currentLootAgent.lootIndicator;
            }
        } 
        else if (isLooting && Input.GetButtonUp(lootActionButtonName))
        {
            if(currentLootIndicator != null)
                currentLootIndicator.SetState(0);
            
            isLooting           = false;
            lootingProgress     = 0;
        }
    }
    
    private void UpdateLootState()
    {
        if (isLooting)
        {
            lootingProgress += Time.deltaTime;
            lootingProgress = Mathf.Clamp01(lootingProgress);
            
            // Update indicator
            currentLootIndicator.SetState(Mathf.InverseLerp(0f, 1f, lootingProgress));

            if (lootingProgress >= 1f && currentLootAgent)
            {
                currentLootAgent.Loot();
                currentLootAgent     = null;
                currentLootIndicator = null;
                isLooting            = false;
            }
        }
    }
    
    private EnemyAI SelectLootTarget()
    {
        List<EnemyAI> agentsDead = AIManager.instance.GetAllAgentsDead()
            .OrderBy(x => (x.transform.position - transform.position).sqrMagnitude).ToList();
      
        currentLootAgent     = null;
        currentLootIndicator = null;
        
        foreach (EnemyAI agent in agentsDead)
        {
            if (agent == null)
                continue;

            // Skip if there is nothing to loot
            if (agent.isLooted)
                continue;

            Vector3 ownPosition   = transform.position;
            Vector3 agentCenter   = agent.GetComponentInChildren<Renderer>().bounds.center;
            Vector3 delta         = agentCenter - ownPosition;
            float distanceSqr     = delta.sqrMagnitude;

            // The agent should be within loot distance
            if (distanceSqr > lootDistance * lootDistance)
                continue;

            // The agent should be within our field of view
            if (Vector3.Angle(delta, transform.forward) > lootMaxAngle * 0.5f)
                continue;

            return agent;
        }

        return null;
    }
}
