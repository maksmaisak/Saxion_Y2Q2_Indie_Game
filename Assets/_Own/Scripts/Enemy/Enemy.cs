using System;
using System.Collections;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Health))]
public class Enemy : MonoBehaviour
{
	[SerializeField] bool enableAI = true;

	/********* PUBLIC *********/
	public EnemyAI AI { get; private set; }

	private void OnValidate()
	{
		AI = GetComponent<EnemyAI>();
		
		if (enableAI)
		{
			if (AI != null)
			{
				UnityEditor.EditorApplication.delayCall+=()=>
				{
					DestroyImmediate(AI);
					AI = null;
				};
			}
			else AI = gameObject.AddComponent<EnemyAI>();
		}
		else
		{
			UnityEditor.EditorApplication.delayCall+=()=>
			{
				if (AI != null)
					DestroyImmediate(AI);
				
				AI = null;
			};
		}
	}	
}
