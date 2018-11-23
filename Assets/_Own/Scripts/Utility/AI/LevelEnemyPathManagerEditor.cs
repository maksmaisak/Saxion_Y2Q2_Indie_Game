using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

/// Editor script for LevelEnemyPathManager
[CustomEditor(typeof(EnemyStatePatrol))]
public class LevelEnemyPathManagerEditor : Editor {

	void OnSceneGUI() {

		var pathManager = target as EnemyStatePatrol;
		var points = pathManager.waypoints;

		Handles.DrawAAPolyLine(3f, points.ToArray());

		for (int i = 0; i < points.Count; ++i) {

			EditorGUI.BeginChangeCheck();
			var newValue = Handles.DoPositionHandle(points[i], Quaternion.identity);
			bool didChange = EditorGUI.EndChangeCheck();

			if (didChange) {
				
				Undo.RecordObject(pathManager, "Move enemy path waypoint");
				EditorUtility.SetDirty(pathManager);

				points[i] = newValue;
			}
		}
	}
}
