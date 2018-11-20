using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using  Cinemachine.Utility;
using Unity.Collections;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent), typeof(Health))]
public class EnemyAI : MonoBehaviour {
	
	[Header("AI Search Settings")]
	[SerializeField] LayerMask blockingLayerMask 	= Physics.DefaultRaycastLayers;
	[SerializeField] float maxViewAngle 			= 60.0f;
	[SerializeField] float maxViewDistance 			= 20.0f;
	
	/********* PUBLIC *********/
	public Health health {get; private set;}
	
	public FSM<EnemyAI> fsm {get; private set;}
	public NavMeshAgent navMeshAgent { get; private set; }
	
	public Vector3? lastHeardDisturbancePositions { get; set; }
	public Vector3 lastKnownPlayerPosition  { get; private set; }
	
	public Transform targetTransform { get; private set; }
	public bool isPlayerVisible { get; private set; }
	public FovInfo fov { get; private set; }
	
	/********* PRIVATE *********/
	
	void Start()
	{		
		targetTransform = FindObjectOfType<PlayerShootingController>()?.transform;
		Assert.IsNotNull(targetTransform);

		fsm 			= new FSM<EnemyAI>(this);
		health 			= GetComponent<Health>();
		navMeshAgent 	= GetComponent<NavMeshAgent>();
		fov 			= new FovInfo {
			maxAngle = maxViewAngle, maxDistance = maxViewDistance, layerMask = blockingLayerMask
		};
		
		health.OnDeath += sender => new Disturbance(transform.position).PostEvent();
		
		fsm.ChangeState<EnemyStateIdle>();
	}

	public void Update()
	{
		LookForPlayer();
	}

	public void LookForPlayer()
	{
		isPlayerVisible = Visibility.CanSeeObject(transform, targetTransform, fov);

		if (isPlayerVisible)
		{
			lastKnownPlayerPosition = targetTransform.position;
			fsm.ChangeState<EnemyStateDistracted>();
		}
	}
}
