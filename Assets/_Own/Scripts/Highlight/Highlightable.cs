using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;

public class Highlightable : MonoBehaviour
{	
	[Tooltip("The highlight can't appear if the object is further away than this from the camera.")]
	[SerializeField] float maxDistance;
	[SerializeField] private LayerMask raycastLayerMask = Physics.DefaultRaycastLayers;
	[Tooltip("The highlight only shows up if the object is within this rectangle on the screen, defined in normalized viewport coordinates.")]
	[SerializeField] Rect requiredViewportRect;
	[SerializeField] float sizeMultiplier = 1f;
	[SerializeField] HUDHighlightIndicator hudElementPrefab; 
	
	private HUDHighlightIndicator hudElement;

	private new Camera camera;
	private Transform cameraTransform;
	private new Renderer renderer;
		
	void Start()
	{
		camera = Camera.main;
		Assert.IsNotNull(camera);
		cameraTransform = camera.transform;
		
		renderer = GetComponentInChildren<Renderer>();
		Assert.IsNotNull(renderer);
		
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
		Ray ray = new Ray(cameraPosition, position - cameraPosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, maxDistance, raycastLayerMask) && hit.collider.gameObject != gameObject && !hit.collider.transform.IsChildOf(transform))
		{
			if (hit.distance < Vector3.Distance(cameraPosition, position))
			{
				hudElement.gameObject.SetActive(false);
				return;
			}
		}

		Rect viewportBounds = renderer.GetViewportBounds(camera);
		Vector2 minViewportPosition = viewportBounds.min;
		Vector2 maxViewportPosition = viewportBounds.max;
		Vector2 centerViewportPosition = viewportBounds.center;

		minViewportPosition = centerViewportPosition + (minViewportPosition - centerViewportPosition) * sizeMultiplier;
		maxViewportPosition = centerViewportPosition + (maxViewportPosition - centerViewportPosition) * sizeMultiplier;

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
