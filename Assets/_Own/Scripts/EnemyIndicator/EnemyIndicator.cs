using System;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyIndicator : MonoBehaviour
{	
	[SerializeField] Color colorIdle = Color.white;
	[SerializeField] Color colorSuspicious = Color.yellow;
	[SerializeField] Color colorAggressive = Color.red;
	[SerializeField] float desiredScreenSize = 0.01f;
	
	private SpriteRenderer spriteRenderer;
	private Transform cameraTransform;
	
	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		Assert.IsNotNull(spriteRenderer);
	}
	
	void Update()
	{
		if (!spriteRenderer) return;
		if (Mathf.Approximately(desiredScreenSize, 0f)) return;
		
		var camera = Camera.main;

		float diameter = spriteRenderer.bounds.size.x;
		float distance = Vector3.Distance(camera.transform.position, transform.position);
		float onScreenSize = diameter / (distance * camera.fieldOfView * Mathf.Deg2Rad);
		transform.localScale *= desiredScreenSize / onScreenSize;
	}

	void OnBecameVisible()
	{
		Debug.Log(this + " became visible.");
	}

	private void OnBecameInvisible()
	{
		Debug.Log(this + " became invisible.");
	}

	public void SetStateIdle()
	{
		spriteRenderer.color = colorIdle;
	}

	public void SetStateSuspicious()
	{
		spriteRenderer.color = colorSuspicious;
	}

	public void SetStateAggressive()
	{
		spriteRenderer.color = colorAggressive;
	}
}
