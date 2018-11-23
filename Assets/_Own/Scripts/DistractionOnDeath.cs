using System;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Health))]
public class DistractionOnDeath : MonoBehaviour 
{
	void Start()
	{
		var health = GetComponent<Health>();
		health.OnDeath += sender =>
		{
			new Distraction(transform.position).PostEvent();
		};
	}
}
