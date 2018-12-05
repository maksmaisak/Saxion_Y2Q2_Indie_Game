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
    
    private Vector3 currentWaypoint = Vector3.zero;
    
    private void OnEnable()
    {
        agent.minimumAwarenessLevelThreshold = 0f;
        agent.navMeshAgent.speed  = agent.patrolSpeed;

        if (waypoints.Count == 0)
        {
            Debug.LogWarningFormat("Did not found waypoints for agent with name {0} !", agent.name);
            agent.fsm.ChangeState<EnemyStateIdle>();
            return;
        }
 
        if (currentWaypoint.Equals(Vector3.zero))
            currentWaypoint = waypoints[waypointIndex];

        agent.navMeshAgent.SetDestination(currentWaypoint);

        StartCoroutine(FollowWaypoints());
    }

    private void OnDisable() => StopAllCoroutines();

    IEnumerator FollowWaypoints()
    {
        while (enabled)
        {
            if (!agent.navMeshAgent.pathPending &&
                agent.navMeshAgent.remainingDistance <=
                Mathf.Max(agent.navMeshAgent.stoppingDistance, waypointStoppingDistance)
            )
            {
                currentWaypoint = GetNextWaypoint();
                agent.navMeshAgent.SetDestination(currentWaypoint);
            }
            
            yield return null;
        }
    }

    Vector3 GetNextWaypoint()
    {
        waypointIndex = (waypointIndex + 1) % waypoints.Count;
        return waypoints[waypointIndex];
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    #if UNITY_EDITOR
        // Prevent saving if this is a prefab
        PrefabType prefabType = PrefabUtility.GetPrefabType(this);
        if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab )
            waypoints = null;
    #endif
    }
    
    void ISerializationCallbackReceiver.OnAfterDeserialize() {}
}
