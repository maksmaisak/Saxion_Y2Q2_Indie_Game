using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBuilder
{
	public static GameObject CreateAndAddObjectToCanvas(GameObject prefab)
	{
		var canvas = LevelCanvas.instance.transform;
		var gameObject = GameObject.Instantiate(prefab, canvas.transform);

		return gameObject;
	}
}
