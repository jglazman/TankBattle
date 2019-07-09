using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Glazman.Tank
{
	public enum TileType
	{
		Sand = 0,
		DirtRoad_NEWS2,
		DirtRoad_NEWS,
		DirtRoad_ES,
		DirtRoad_SW,
		DirtRoad_NE,
		DirtRoad_NW,
		HalfRoad_SN,
		HalfRoad_NS,
		HalfRoad_SN2,

		Sand2,
		DirtRoad_NS,
		DirtRoad_EW,
		DirtRoad_NES,
		DirtRoad_NSW,
		DirtRoad_NEW,
		DirtRoad_ESW,
		HalfRoad_WE,
		HalfRoad_EW,
		HalfRoad_WE2,
			
		Grass,
		Road_NEWS2,
		Road_NEWS,
		Road_ES,
		Road_SW,
		Road_NE,
		Road_NW,
		HalfGrass_SN,
		HalfGrass_NS,
		HalfRoad_NS2,
			
		Grass2,
		Road_NS,
		Road_EW,
		Road_NES,
		Road_NSW,
		Road_NEW,
		Road_ESW,
		HalfGrass_WE,
		HalfGrass_EW,
		HalfRoad_EW2,			
	}
	
	public class TileMaterial : MonoBehaviour
	{
		[SerializeField] private TileType _tileType;
		[SerializeField] private Renderer _renderer;

		public TileType TileType => _tileType;

		// hardcoded to terrainTiles_default.png
		private const float TILE_SIZE = 64f;
		private const float TILES_WIDE = 10f;
		private const float TILES_HIGH = 4f;

//		private void Awake()
//		{
//			SetTileType(_tileType);
//		}
		
		public void SetTileType(TileType type)
		{
			_tileType = type;

			if (_renderer == null)
				return;

			float xTile = (int)_tileType % TILES_WIDE;
			float yTile = Mathf.FloorToInt((int)_tileType / TILES_WIDE);
			
			_renderer.sharedMaterial.mainTextureOffset = new Vector2(
				(xTile * TILE_SIZE) / (TILES_WIDE * TILE_SIZE),
				(yTile * TILE_SIZE) / (TILES_HIGH * TILE_SIZE) );
			
			_renderer.sharedMaterial.mainTextureScale = new Vector2(
				TILE_SIZE / (TILES_WIDE * TILE_SIZE),
				TILE_SIZE / (TILES_HIGH * TILE_SIZE) );
		}
	}
}
