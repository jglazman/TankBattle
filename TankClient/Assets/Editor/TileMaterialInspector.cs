using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Glazman.Tank
{
	[CustomEditor(typeof(TileMaterial))]
	public class TileMaterialInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUI.changed)
			{
				var tileMaterial = target as TileMaterial;
				var newTileType = tileMaterial.TileType;
				tileMaterial.SetTileType(newTileType);
				EditorUtility.SetDirty(target);
			}
		}
	}
}
