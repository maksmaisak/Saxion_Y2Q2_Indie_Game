using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBuilder
{
	public static T CreateAndAddObjectToCanvas<T>(T prefab) where T : UnityEngine.Object
	{
		var canvas = LevelCanvas.instance;
		var gameObject = Object.Instantiate(prefab, canvas.transform);

		return gameObject;
	}
}
