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
		
		Rect viewportBounds = renderer.GetViewportBounds(camera);
		if (!IsVisible(GetRaysToObjectInViewport(viewportBounds)))
		{
			hudElement.gameObject.SetActive(false);
			return;
		}
		
		var rectTransform = hudElement.GetComponent<RectTransform>();
		Vector2 centerViewportPosition = viewportBounds.center;
		Vector2 minViewportPosition = viewportBounds.min;
		Vector2 maxViewportPosition = viewportBounds.max;
		minViewportPosition = centerViewportPosition + (minViewportPosition - centerViewportPosition) * sizeMultiplier;
		maxViewportPosition = centerViewportPosition + (maxViewportPosition - centerViewportPosition) * sizeMultiplier;
		rectTransform.anchorMin = minViewportPosition;
		rectTransform.anchorMax = maxViewportPosition;
		rectTransform.anchoredPosition = Vector2.zero;
		
		hudElement.gameObject.SetActive(true);
	}

	private IEnumerable<Ray> GetRaysToObjectInViewport(Rect viewportBounds)
	{
		Rect bounds = ScaleRect(viewportBounds, 0.8f);
		return new[]
		{
			bounds.min,
			new Vector2(bounds.min.x, bounds.max.y),
			new Vector2(bounds.max.x, bounds.min.y),
			bounds.max,	
			bounds.center
		}
		.Select(p => camera.ViewportPointToRay(p));
	}

	private static Rect ScaleRect(Rect rect, float multiplier)
	{
		Vector2 center = rect.center;
		Vector2 min = center + (rect.min - center) * multiplier;
		Vector2 max = center + (rect.max - center) * multiplier;
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}

	/// Assuming the object is closer than maxDistance
	private bool IsVisible(IEnumerable<Ray> rays)
	{
		foreach (Ray ray in rays)
		{
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, maxDistance, blockingLayers, QueryTriggerInteraction.Ignore))
			{
				if (hit.collider.gameObject == gameObject) return true;
				if (hit.collider.transform.IsChildOf(transform)) return true;
				if (transform.IsChildOf(hit.collider.transform)) return true;
			}
			else
			{
				return true;
			}
		}

		return false;
	}
}
