using System;
using UnityEngine;

public class DestroyWhenOutOfBounds : MonoBehaviour
{
    [SerializeField] Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2000f);
	
	void Update()
	{
		if (!bounds.Contains(transform.position)) Destroy(gameObject);
	}
}
