using System;
using UnityEngine;

public class DetachAndResetTransformOfChildren : MonoBehaviour
{
	void Start()
	{
		foreach (Transform child in transform)
		{
			child.SetParent(null, worldPositionStays: false);
		}
	}
}
