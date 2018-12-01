using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CanvasObjectBuilder
{
	public static T CreateAndAddObjectToCanvas<T>(T prefab) where T : UnityEngine.Object
	{
		var canvas = LevelCanvas.instance;
		Transform parentTransform = canvas.indicatorsRoot ? canvas.indicatorsRoot.transform : canvas.transform;
		var gameObject = Object.Instantiate(prefab, parentTransform);

		return gameObject;
	}
}
