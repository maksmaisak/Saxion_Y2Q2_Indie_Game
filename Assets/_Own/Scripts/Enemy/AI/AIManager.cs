using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : Singleton<AIManager>, IEventReceiver<Distraction>, IEventReceiver<OnEnemyCombat>
{
    private int nextAIEntry = 0;

    private readonly List<EnemyAI> agents                 = new List<EnemyAI>();
    private readonly List<EnemyAI> agentsInCombat         = new List<EnemyAI>();
    private readonly List<EnemyAI> agentsDead             = new List<EnemyAI>();
    private readonly List<Investigation> investigations   = new List<Investigation>();

    public void RegisterDeadAgent(EnemyAI agent) => agentsDead.Add(agent);
    public void RegisterAgent(EnemyAI newAgent) => agents.Add(newAgent);
    public void UnregisterAgent(EnemyAI newAgent) => agents.Remove(newAgent);

    public EnemyAI GetEnemyAIWithId(int aiGUID) => agents.FirstOrDefault(x => x.aiGUID == aiGUID);

    public int GetNextAssignableEntryId()
    {
        nextAIEntry++;
        return nextAIEntry;
    }

    public List<EnemyAI> GetAllAgents() => agentsDead;
    public List<EnemyAI> GetAllAgentsInCombat() => agentsInCombat;
    public List<EnemyAI> GetAllAgentsDead() => agentsDead;

    public List<EnemyAI> GetAllAssistAgentsInRange(EnemyAI initiator, float radius, bool checkLineOfSight = false)
    {
        List<EnemyAI> assistList = new List<EnemyAI>();

        foreach (EnemyAI agent in agents)
        {
            if (!agent || agent == initiator)
                continue;

            if (!agent.CanAssist())
                continue;

            Vector3 delta = (agent.transform.position - initiator.transform.position);

            if (delta.sqrMagnitude > radius * radius)
                continue;

            if (checkLineOfSight)
            {
                Ray ray = new Ray(initiator.visionOriginTransform.position, delta.normalized);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, delta.magnitude + 1.0f))
                    if (hit.distance * hit.distance < delta.sqrMagnitude)
                        continue;
            }

            assistList.Add(agent);
        }

        return assistList;
    }

    public void On(OnEnemyCombat combat)
    {
        if (combat.enterCombat)
            agentsInCombat.Add(combat.agent);
        else agentsInCombat.Remove(combat.agent);
    }

    public void On(Distraction distraction)
    {
        Investigation investigation = new Investigation(distraction);

        foreach (EnemyAI agent in agents)
            if (agent.CanStartNewInvestigation(investigation))
                investigation.AssignAgent(agent);

        if (investigation.agents.Count == 0)
            return;

        investigation.Start();
        investigation.OnInvestigationFinish += OnInvestigationFinish;
        investigations.Add(investigation);
    }

    private void OnInvestigationFinish(Investigation investigation) => investigations.Remove(investigation);

    private void Start() => StartCoroutine(UpdateInvestigationsCoroutine());

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        StopAllCoroutines();
        agents.Clear();
    }

    private IEnumerator UpdateInvestigationsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            int investigationCount = investigations.Count;

            if(investigationCount == 0)
                continue;
            
            for (int i = investigationCount - 1; i >= 0; i--)
            {
                Investigation investigation = investigations[i];
                if (investigation != null)
                    investigation.Update();
            }
        }
    }
}

   