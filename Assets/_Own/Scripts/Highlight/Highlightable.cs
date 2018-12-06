using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class Highlightable : MonoBehaviour
{
	[Tooltip("The highlight can't appear if the object is further away than this from the camera.")]
	[SerializeField] float maxDistance = 100f;
	[SerializeField] LayerMask blockingLayers = Physics.DefaultRaycastLayers;
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
		Assert.IsNotNull(renderer, this + ": No Renderer found on a Highlightable object. Can't determine bounds on screen.");
		
		Assert.IsNotNull(hudElementPrefab);
		hudElement = CanvasObjectBuilder.CreateAndAddObjectToCanvas(hudElementPrefab);
	}

	private void OnDestroy()
	{
		if (hudElement) Destroy(hudElement.gameObject);
		hudElement = null;
	}

	void LateUpdate()
	{
		Vector3 position = renderer.bounds.center;
		Vector3 viewportPosition = camera.WorldToViewportPoint(position);
		if (viewportPosition.z < 0f || !requiredViewportRect.Contains(viewportPosition))
		{
			hudElement.gameObject.SetActive(false);
			return;
		}

		Vector3 cameraPosition = cameraTransform.position;
		Vector3 delta = position - cameraPosition;
		float distance = delta.magnitude;
		if (distance > maxDistance)
		{
			hudElement.gameObject.SetActive(false);
			return;
		}
		
		Ray ray = new Ray(cameraPosition, delta);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, distance, blockingLayers))
		{
			if (hit.collider.gameObject != gameObject && !hit.collider.transform.IsChildOf(transform) && !transform.IsChildOf(hit.collider.transform))
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
	}
}
