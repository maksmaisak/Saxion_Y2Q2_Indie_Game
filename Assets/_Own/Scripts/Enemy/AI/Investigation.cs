using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Investigation
{
    public float startTime { get; }
    public List<EnemyAI> agents { get; }
    public Vector3 distractionPoint { get; }
    public float distractionLoudness { get; }
    public float priority { get; }

    public delegate void OnFinishInvestigation(Investigation investigation);
    public event OnFinishInvestigation OnInvestigationFinish;
    
    public Investigation(Distraction distraction) {
        distractionPoint            = distraction.position;
        priority                    = distraction.priority;
        this.distractionLoudness    = distraction.loudness;
        agents                      = new List<EnemyAI>();
        startTime                   = Time.time;
    }

    public void AssignAgent(EnemyAI agent) => agents.Add(agent);

    public void Finish() => OnInvestigationFinish?.Invoke(this);
    
    public void Start()
    {
        // For every 3 agents per investigation select one extra agent to allow him to wander further after investigation is finished
        // besides the main one.
        int wanderAgentsCount = 1 + (agents.Count / 3);

        List<EnemyAI> agentsSortedByDistance = agents.OrderBy(x => (distractionPoint - x.visionOriginTransform.position).sqrMagnitude).ToList();
        for(int i = 0; i < wanderAgentsCount; i++)
        {
            EnemyAI agent = agentsSortedByDistance[i];
            if (agent == null)
                continue;

            agent.SetWanderAfterInvestigation(true);
        }

        // Start investigation for each agent
        foreach (EnemyAI agent in agentsSortedByDistance)
            agent.Investigate(this);
    }

    public void Update()
    {
        List<EnemyAI> agentsToRemove = new List<EnemyAI>();

        foreach (EnemyAI agent in agents)
        {
            // Agents might have died and have been destroyed during investigation
            if (agent == null)
            {
                agentsToRemove.Add(agent);
                continue;
            }

            // Agents might have following another investigation or finished doing the investigation
            if (!agent.isInvestigating || (agent.isInvestigating && agent.currentInvestigation != this))
                agentsToRemove.Add(agent);
        }

        foreach (EnemyAI agent in agentsToRemove)
            agents.Remove(agent);

        // If there is no agent left just close this investigation
        if (agents.Count == 0)
            Finish();
    }
}