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
		
		SetRigidbodiesActive(false);
		GetComponent<Health>().OnDeath += sender =>
		{			
			SetRigidbodiesActive(true);
			animator.enabled = false;
		};
	}

	void SetRigidbodiesActive(bool isActive)
	{		
		foreach (var rb in rigidbodies)
		{
			rb.isKinematic = !isActive;
		}
	}
}
