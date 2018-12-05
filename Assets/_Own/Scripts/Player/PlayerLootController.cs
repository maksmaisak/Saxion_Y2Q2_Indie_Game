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

    /********* PRIVATE *********/
    private bool isLooting;
    private float lootingProgress;
    
    private EnemyLootable currentLoot = null;
    private EnemyLootIndicator currentLootIndicator = null;   
    
    private void Update()
    {
        UpdateLootInput();
        UpdateLootState();
    }

    private void UpdateLootInput()
    {        
        if (Input.GetButtonDown(lootActionButtonName))
        {            
            currentLoot  = SelectLootTarget();

            if (currentLoot != null)
            {                
                lootingProgress       = 0;
                isLooting             = true;
                currentLootIndicator  = currentLoot.lootIndicator;
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

            if (lootingProgress >= 1f && currentLoot)
            {
                currentLoot.Loot();
                currentLoot     = null;
                currentLootIndicator = null;
                isLooting            = false;
            }
        }
    }
    
    private EnemyLootable SelectLootTarget()
    {
        List<EnemyAI> agentsDead = AIManager.instance.GetAllAgentsDead()
            .OrderBy(x => (x.transform.position - transform.position).sqrMagnitude).ToList();
      
        currentLoot     = null;
        currentLootIndicator = null;
        
        foreach (EnemyAI agent in agentsDead)
        {
            if (agent == null)
                continue;

            EnemyLootable loot = agent.GetComponent<EnemyLootable>();
            if(!loot)
                continue;
            
            // Skip if there is nothing to loot
            if (loot.isLooted)
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

            return loot;
        }

        return null;
    }
}
