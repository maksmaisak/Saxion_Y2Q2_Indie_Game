using System;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

[RequireComponent(typeof(NavMeshAgent), typeof(Health))]
public class Enemy : MonoBehaviour
{	
	[SerializeField] FovDisplay fovDisplay;
	[SerializeField] Transform playerTransform;
	
	public FSM<Enemy> fsm {get; private set;}
	public Health health {get; private set;}
	public NavMeshAgent navMeshAgent { get; private set; }
	public FovInfo currentFov = new FovInfo {maxAngle = 70f, maxDistance = 30f, blockingLayerMask = Physics.DefaultRaycastLayers};
	public Vector3? lastHeardDisturbancePositions { get; set; }
	
	public Vector3 lastKnownPlayerPosition  { get; private set; }
	public bool isPlayerVisible { get; private set; }

	void Start()
	{
		Assert.IsNotNull(fovDisplay);

		playerTransform = playerTransform
			? playerTransform
			: FindObjectOfType<PlayerShootingController>()?.transform;
		
		fsm = new FSM<Enemy>(this);
		health = GetComponent<Health>();
		navMeshAgent = GetComponent<NavMeshAgent>();
		
		fsm.ChangeState<EnemyStateIdle>();
		health.OnDeath += sender => new Disturbance(transform.position).PostEvent();
	}

	void OnValidate()
	{
		UpdateFovDisplay();
	}

	void Update()
	{
		UpdateFovDisplay();
		isPlayerVisible = PlayerVisible();
		if (isPlayerVisible)
		{
			lastKnownPlayerPosition = playerTransform.position;
			fsm.ChangeState<EnemyStateChasePlayer>();
		}
	}

	private void UpdateFovDisplay()
	{
		fovDisplay.fov = currentFov;
	}

	private bool PlayerVisible()
	{
		Vector3 ownPosition = transform.position;
		Vector3 toPlayer = playerTransform.position - ownPosition;
		toPlayer = toPlayer.ProjectOntoPlane(Vector3.up);

		if (toPlayer.sqrMagnitude > currentFov.maxDistance * currentFov.maxDistance) return false;
		if (Vector3.Angle(toPlayer, transform.forward) > currentFov.maxAngle * 0.5f) return false;

		RaycastHit hit;
		if (!Physics.Raycast(ownPosition, toPlayer, out hit, currentFov.maxDistance, currentFov.blockingLayerMask)) 
			return true;
		
		return hit.distance * hit.distance > toPlayer.sqrMagnitude;
	}
}
