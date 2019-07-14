using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Glazman.Tank
{
	[CustomEditor(typeof(TerrainBehaviour))]
	public class TerrainBehaviourInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUI.changed)
			{
				var terrain = target as TerrainBehaviour;
				var newTileType = terrain.TileType;
				terrain.SetTileType(newTileType, false);
				EditorUtility.SetDirty(target);
			}
		}
	}
}
