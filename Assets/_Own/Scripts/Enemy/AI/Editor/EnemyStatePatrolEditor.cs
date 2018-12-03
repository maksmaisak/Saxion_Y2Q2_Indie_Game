using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
	
/// Editor script for LevelEnemyPathManager
[CustomEditor(typeof(EnemyStatePatrol))]
public class EnemyStatePatrolEditor : Editor {

	void OnSceneGUI() {

		var patrol = target as EnemyStatePatrol;
		var points = patrol.waypoints;

		Handles.DrawAAPolyLine(3f, points.ToArray());

		for (int i = 0; i < points.Count; ++i) {

			EditorGUI.BeginChangeCheck();
			var newValue = Handles.DoPositionHandle(points[i], Quaternion.identity);
			bool didChange = EditorGUI.EndChangeCheck();

			if (didChange) {
				
				Undo.RecordObject(patrol, "Move enemy path waypoint");
				EditorUtility.SetDirty(patrol);

				points[i] = newValue;
			}
		}
	}
}

#endif