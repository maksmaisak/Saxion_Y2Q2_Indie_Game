using System;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Animator))]
public class RagdollOnDeath : MonoBehaviour
{
	private Rigidbody[] rigidbodies;
	private Animator animator;
	
	void Start ()
	{		
		rigidbodies = GetComponentsInChildren<Rigidbody>(includeInactive: true);
		animator = GetComponent<Animator>();
		
		SetRigidbodiesActive(true);
		GetComponent<Health>().OnDeath += sender =>
		{
			TimeHelper.timeScale = 0.001f;
			
			SetRigidbodiesActive(false);
			animator.enabled = false;
		};

		GetComponent<Health>().DealDamage(100);
	}

	void SetRigidbodiesActive(bool isActive)
	{		
		foreach (var rb in rigidbodies)
		{
			rb.isKinematic = isActive;
			rb.detectCollisions = isActive;
		}
	}
}
