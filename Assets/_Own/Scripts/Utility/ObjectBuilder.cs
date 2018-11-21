using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBuilder
{
	public static GameObject CreateAndAddObjectToCanvas(GameObject prefab)
	{
		var canvas = GameObject.FindObjectOfType<Canvas>();
		var gameObject = GameObject.Instantiate(prefab, canvas.transform);

		return gameObject;
	}
}
