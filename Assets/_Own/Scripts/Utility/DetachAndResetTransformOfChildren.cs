using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachAndResetTransformOfChildren : MonoBehaviour
{
	void Start()
	{
		foreach (Transform child in GetChildren().ToArray())
		{
			child.SetParent(null, worldPositionStays: false);
		}
	}

	private IEnumerable<Transform> GetChildren()
	{
		foreach (Transform child in transform)
			yield return child;
	}
}
