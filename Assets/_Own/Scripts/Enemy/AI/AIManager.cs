using System;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : Singleton<AIManager>
{
   private int nextAIEntry = 0;
   
   private readonly Dictionary<int, EnemyAI> agents                = new Dictionary<int, EnemyAI>();   
   private readonly Dictionary<int, EnemyAI> wanderingAgents       = new Dictionary<int, EnemyAI>();

   private readonly Dictionary<InvestigateReason, List<EnemyAI>> reasonInvestigatorAgents =
      new Dictionary<InvestigateReason, List<EnemyAI>>();

   public void RegisterAgent(EnemyAI newAgent) => agents.Add(newAgent.aiGUID, newAgent);
   public void UnregisterAgent(EnemyAI newAgent) => agents.Remove(newAgent.aiGUID);

   public void AssignInvestigator(EnemyAI investigator)
   {
      List<EnemyAI> agents;
      // If there are no agents assigned to this investigation
      // assign them now.
      if (!reasonInvestigatorAgents.TryGetValue(investigator.currentInvestigateReason, out agents))
      {
         agents = new List<EnemyAI> {investigator};
         this.reasonInvestigatorAgents.Add(investigator.currentInvestigateReason, agents);
      }
      else agents.Add(investigator);
   }

   public void UnassignInvestigator(EnemyAI investigator)
   {
      List<EnemyAI> agents;
      if (!reasonInvestigatorAgents.TryGetValue(investigator.currentInvestigateReason, out agents))
         return;

      reasonInvestigatorAgents.Remove(investigator.currentInvestigateReason);
   }

   public void RegisterWanderer(EnemyAI wanderer) => wanderingAgents.Add(wanderer.aiGUID, wanderer);
   public void UnregisterWanderer(EnemyAI wanderer) => wanderingAgents.Remove(wanderer.aiGUID);

   public int GetNextAssignableEntryId()
   {
      nextAIEntry++;
      return nextAIEntry;
   }

   public EnemyAI GetEnemyAIWithId(int id)
   {
      EnemyAI enemyAI;

      if (!agents.TryGetValue(id, out enemyAI))
         return null;
      
      return enemyAI;
   }

   public bool CanWander(EnemyAI agent)
   {
      return true;
      
      InvestigateReason reason = agent.currentInvestigateReason;

      if (reason == InvestigateReason.PlayerSeen)
         return true;
      
      foreach (var kvp in wanderingAgents)
      {
         // Should not happen
         if (kvp.Value == agent)
            return false;
         
         if (kvp.Value.currentInvestigateReason == reason)
            return false;
      }

      return true;
   }
}