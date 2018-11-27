using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Investigation
{
    public float startTime { get; }
    public List<EnemyAI> agents { get; }
    public Vector3 distractionPoint { get; }
    public float priority { get; }

    public delegate void OnFinishInvestigation(Investigation investigation);
    public event OnFinishInvestigation OnInvestigationFinish;
    
    public Investigation(Distraction distraction) {
        distractionPoint    = distraction.position;
        priority            = distraction.distractionPriority;
        agents              = new List<EnemyAI>();
        startTime           = Time.time;
    }

    public void AssignAgent(EnemyAI agent) => agents.Add(agent);

    public void Finish() => OnInvestigationFinish?.Invoke(this);
    
    public void Start()
    {
        float distanceSqr    = Mathf.Infinity;
        EnemyAI closestAgent = null;
        
        foreach (EnemyAI agent in agents)
        {
            // Make the closest agent the leader, allow him to wander further after investigation is finished.
            float newDistanceSqr = (distractionPoint - agent.visionOriginTransform.position).sqrMagnitude;
            
            if (newDistanceSqr < distanceSqr) {
                closestAgent = agent;
                distanceSqr  = newDistanceSqr;
            }
            
            agent.Investigate(this);
        }

        if (closestAgent != null)
            closestAgent.SetWanderAfterInvestigation(true);
    }

    public void Update()
    {
        List<EnemyAI> agentsToRemove = new List<EnemyAI>();
        
        foreach (EnemyAI agent in agents)
        {
            // Agents might have died and have been destroyed during investigation
            if (agent == null) {
                agentsToRemove.Add(agent);
                continue;
            }

            // Agents might have following another investigation or finished doing the investigation
            if(!agent.isInvestigating || (agent.isInvestigating && agent.currentInvestigation != this))
                agentsToRemove.Add(agent);
        }

        foreach (EnemyAI agent in agentsToRemove)
            agents.Remove(agent);
        
        // If there is no agent left just close this investigation
        if(agents.Count == 0)
            Finish();
    }
}