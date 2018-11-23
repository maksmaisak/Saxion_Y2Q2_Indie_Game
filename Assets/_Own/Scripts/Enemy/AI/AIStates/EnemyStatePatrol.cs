using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class EnemyStatePatrol : FSMState<EnemyAI>, ISerializationCallbackReceiver
{
    [SerializeField] float waypointStoppingDistance = 1f;
    
    /********* PUBLIC *********/
    public List<Vector3> waypoints = new List<Vector3>();
    
    /********* PRIVATE *********/
    private int waypointIndex = 0;
    private bool isCurrentWaypointReached = false;
    
    private Vector3 currentWaypoint = Vector3.zero;
    
    private void OnEnable()
    {      
        agent.minimumTimeTreshold = 0f;
        agent.navMeshAgent.speed  = agent.patrolSpeed;
        
        if (waypoints.Count == 0)
        {
            Debug.LogErrorFormat("Did not found waypoints for agent with name {0} !", agent.name);
            return;
        }

        isCurrentWaypointReached         = false;
        agent.canInvestigateDisturbance  = true;
        
        if (currentWaypoint.Equals(Vector3.zero))
            currentWaypoint = waypoints[waypointIndex];
        
        agent.navMeshAgent.SetDestination(currentWaypoint);
        
        StartCoroutine(FollowWaypoints());
    }

    private void OnDisable()
    {
        agent.canInvestigateDisturbance = false;
        
        StopAllCoroutines();
    }

    IEnumerator FollowWaypoints()
    {
        while (enabled)
        {
            if ((waypoints[waypointIndex] - agent.transform.position).sqrMagnitude <=
                waypointStoppingDistance * waypointStoppingDistance)
                isCurrentWaypointReached = true;
            
            if (isCurrentWaypointReached)
            {
                isCurrentWaypointReached = false;
                currentWaypoint = GetNextWaypoint();
                agent.navMeshAgent.SetDestination(currentWaypoint);
            }
            
            yield return null;
        }
    }

    Vector3 GetNextWaypoint()
    {
        waypointIndex++;

        if (waypointIndex >= waypoints.Count - 1)
            waypointIndex = 0;

        return waypoints[waypointIndex];
    }
    
    
    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
     #if UNITY_EDITOR
        // Prevent saving if this is a prefab
        PrefabType prefabType = PrefabUtility.GetPrefabType(this);
        if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab )
        {
            waypoints = null;
        }
    #endif
    }
    
    void ISerializationCallbackReceiver.OnAfterDeserialize() {}
}
