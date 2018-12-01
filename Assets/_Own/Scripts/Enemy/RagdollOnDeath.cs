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
		
		SetKinematic(true);
		GetComponent<Health>().OnDeath += sender =>
		{
			SetKinematic(false);
			animator.enabled = false;
		};
	}

	void SetKinematic(bool isKinematic)
	{		
		foreach (var rb in rigidbodies)
		{
			rb.isKinematic = isKinematic;
		}
	}
}
