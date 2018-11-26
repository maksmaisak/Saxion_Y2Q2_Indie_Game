using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : Singleton<AIManager>
{
    private int nextAIEntry = 0;

    private readonly List<EnemyAI> agents = new List<EnemyAI>();
    private readonly Dictionary<int, EnemyAI> wanderingAgents = new Dictionary<int, EnemyAI>();

    public void RegisterAgent(EnemyAI newAgent) => agents.Add(newAgent);
    public void UnregisterAgent(EnemyAI newAgent) => agents.Remove(newAgent);

    public EnemyAI GetEnemyAIWithId(int aiGUID) => agents.FirstOrDefault(x => x.aiGUID == aiGUID);

    public int GetNextAssignableEntryId()
    {
        nextAIEntry++;
        return nextAIEntry;
    }

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
                    if (hit.distance * hit.distance > delta.sqrMagnitude)
                        continue;
            }

            assistList.Add(agent);
        }

        return assistList;
    }
}