using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using  Cinemachine.Utility;
using Unity.Collections;
using UnityEngine.Assertions;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyAI : MonoBehaviour {
	
	[Header("AI Search Settings")]
	[SerializeField] LayerMask blockingLayerMask 	= Physics.DefaultRaycastLayers;
	[SerializeField] float maxViewAngle 			= 60.0f;
	[SerializeField] float maxViewDistance 			= 20.0f;
	
	/********* PUBLIC *********/
	public Enemy owner { get; private set; }
	public Health health {get; private set;}
	
	public FSM<EnemyAI> fsm {get; private set;}
	public NavMeshAgent navMeshAgent { get; private set; }
	
	public Vector3? lastHeardDisturbancePositions { get; set; }
	public Vector3 lastKnownPlayerPosition  { get; private set; }
	
	public Transform targetTransform { get; private set; }
	public bool isPlayerVisible { get; private set; }
	
	/********* PRIVATE *********/
	private float searchPlayerDiff  = 0.0f;
	private float searchPlayerTimer = 0.05f;
	private FovInfo fov { get; set; }
	
	void Start()
	{
		owner = gameObject.GetComponent<Enemy>();
		Assert.IsNotNull(owner);
		
		targetTransform = FindObjectOfType<PlayerShootingController>()?.transform;
		Assert.IsNotNull(targetTransform);

		fsm 			= new FSM<EnemyAI>(this);
		health 			= GetComponent<Health>();
		navMeshAgent 	= GetComponent<NavMeshAgent>();
		fov 			= new FovInfo {
			maxAngle = maxViewAngle, maxDistance = maxViewDistance, blockingLayerMask = blockingLayerMask
		};
		
		health.OnDeath += sender => new Disturbance(transform.position).PostEvent();
		
		fsm.ChangeState<EnemyStateIdle>();
	}

	public void Update()
	{
		if (searchPlayerDiff >= searchPlayerTimer)
		{
			searchPlayerDiff -= searchPlayerTimer;
			isPlayerVisible = Visibility.CanSeeObject(transform, targetTransform, fov);
		}
		else searchPlayerDiff += Time.deltaTime;
	}
}
