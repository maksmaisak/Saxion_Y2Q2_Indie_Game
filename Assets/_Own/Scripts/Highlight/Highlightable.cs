using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Highlightable : MonoBehaviour
{	
	[Tooltip("The highlight can't appear if the object is further away than this from the camera.")]
	[SerializeField] float maxDistance;
	[SerializeField] private LayerMask blockViewLayerMask = Physics.DefaultRaycastLayers;
	[Tooltip("The highlight only shows up if the object is within this rectangle on the screen, defined in normalized viewport coordinates.")]
	[SerializeField] Rect requiredViewportRect;
	[SerializeField] float sizeMultiplier = 1f;
	[SerializeField] HUDHighlightIndicator hudElementPrefab; 
	
	private HUDHighlightIndicator hudElement;

	private new Camera camera;
	private Transform cameraTransform;
		
	void Start()
	{
		camera = Camera.main;
		Assert.IsNotNull(camera);
		
		cameraTransform = camera.transform;
		
		Assert.IsNotNull(hudElementPrefab);

		hudElement = ObjectBuilder.CreateAndAddObjectToCanvas(hudElementPrefab);
	}

	private void OnDestroy()
	{
		if (hudElement) Destroy(hudElement.gameObject);
		hudElement = null;
	}

	void LateUpdate()
	{
		Vector3 position = transform.position;
		Vector3 viewportPosition = camera.WorldToViewportPoint(transform.position);
		if (viewportPosition.z < 0f || !requiredViewportRect.Contains(viewportPosition))
		{
			hudElement.gameObject.SetActive(false);
			return;
		}

		Vector3 cameraPosition = cameraTransform.position;
		Ray ray = new Ray(cameraTransform.position, position - cameraPosition);
		if (Physics.Raycast(ray, Vector3.Distance(cameraPosition, position), blockViewLayerMask))
		{
			hudElement.gameObject.SetActive(false);
			return;
		}
				
		var renderer = GetComponentInChildren<Renderer>();

		Vector3 min = renderer.bounds.min;
		Vector3 max = renderer.bounds.max;
		Vector2[] viewportBorders = new []{
			new Vector3(min.x, min.y, min.z),
			new Vector3(min.x, min.y, max.z),
			new Vector3(min.x, max.y, min.z),
			new Vector3(min.x, max.y, max.z),
			new Vector3(max.x, min.y, min.z),
			new Vector3(max.x, min.y, max.z),
			new Vector3(max.x, max.y, min.z),
			new Vector3(max.x, max.y, max.z)
		}.Select(b => (Vector2)camera.WorldToViewportPoint(b)).ToArray();
		
		Vector2 minViewportPosition = new Vector2(viewportBorders.Min(b => b.x), viewportBorders.Min(b => b.y));
		Vector2 maxViewportPosition = new Vector2(viewportBorders.Max(b => b.x), viewportBorders.Max(b => b.y));

		Vector2 viewportPosition2D = viewportPosition;
		minViewportPosition = viewportPosition2D + (minViewportPosition - viewportPosition2D) * sizeMultiplier;
		maxViewportPosition = viewportPosition2D + (maxViewportPosition - viewportPosition2D) * sizeMultiplier;

		var rectTransform = hudElement.GetComponent<RectTransform>();
		rectTransform.anchorMin = minViewportPosition;
		rectTransform.anchorMax = maxViewportPosition;
		rectTransform.anchoredPosition = Vector2.zero;
		
		hudElement.gameObject.SetActive(true);

		/*
		float diameter = renderer.bounds.size.magnitude;
		float distance = Vector3.Distance(transform.position, cameraTransform.position);
		float viewportspaceRadius = diameter / (distance * camera.fieldOfView * Mathf.Deg2Rad);
		*/
	}
}
