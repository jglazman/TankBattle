using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Tank
{
	public enum GameState
	{
		Bootstrap,
		MainMenu,
		GameInProgress,
		GameOver
	}

	public enum Difficulty
	{
		Easy = 0,
		Normal,
		Hard
	}
	
	/// <summary>
	/// A simple state machine to manage high-level game flow.
	/// </summary>
	public static class Game
	{
		private static GameState _gameState;
		
		
		public static void Initialize()
		{
//			Assert.raiseExceptions = true;
			
			GameUI.ListenForMessages(HandleUIMessage);
		}

		private static void HandleUIMessage(UIMessage message)
		{
			switch (message.type)
			{
				case UIMessage.MessageType.SplashContinue:
				{
					Assert.IsTrue(_gameState == GameState.Bootstrap);
					
					_gameState = GameState.MainMenu;
					Utilities.LoadScene(SceneName.MainMenu);
				} break;
				
				case UIMessage.MessageType.StartNewGame:
				{
					Assert.IsTrue(_gameState == GameState.MainMenu);
					
					var difficulty = (Difficulty)message.data[0].DropdownIndex;
					StartNewGame(difficulty);
				} break;

				case UIMessage.MessageType.QuitGame:
				{
					Assert.IsTrue(_gameState == GameState.GameInProgress);
					
					EndGame();
				} break;
			}
		}

		private static TileMaterial[,] _terrainTiles;

		private static void StartNewGame(Difficulty difficulty)
		{
			Debug.LogWarning($"Start new game!  Difficulty={difficulty}");

			_gameState = GameState.GameInProgress;
			Utilities.LoadScene(SceneName.Game);

			UnityWrapper.Instance.StartCoroutine(GenerateTerrain(difficulty));
		}

		private const int TERRAIN_NUM_COLS = 19;
		private const int TERRAIN_NUM_ROWS = 19;
		private const int TERRAIN_TILE_WIDTH = 3;
		private const int TERRAIN_TILE_HEIGHT = 3;
		
		private static Vector3 GetWorldPosition()
		{
			return new Vector3(
				( TERRAIN_TILE_WIDTH * TERRAIN_NUM_COLS * -0.5f ) + ( TERRAIN_TILE_WIDTH * TERRAIN_NUM_COLS ) - ( TERRAIN_TILE_WIDTH * 0.5f ),
				0f,
				( TERRAIN_TILE_HEIGHT * TERRAIN_NUM_ROWS * -0.5f ) + ( TERRAIN_TILE_HEIGHT * TERRAIN_NUM_ROWS ) - ( TERRAIN_TILE_HEIGHT * 0.5f )
			);
		}
		
		private static IEnumerator GenerateTerrain(Difficulty difficulty)
		{
			WorldTileConfig[,] worldSeed;
			switch (difficulty)
			{
				case Difficulty.Easy:
					worldSeed = WorldGenSeed.SEED_MAZE2;
					break;
				
				case Difficulty.Hard:
					worldSeed = WorldGenSeed.SEED_CAVERN1;
					break;
				
				case Difficulty.Normal:
				default:
					worldSeed = WorldGenSeed.SEED_MAZE1;
					break;
			}
			
			int randomSeed = UnityEngine.Random.Range( 0, 9999999 );
			var config = new TerrainGenerator.WorldGenConfig( randomSeed, WorldType.Prim, TERRAIN_NUM_COLS, TERRAIN_NUM_ROWS, new Vector2( TERRAIN_TILE_WIDTH, TERRAIN_TILE_HEIGHT ), worldSeed );
			
			var terrainGenerator = new TerrainGenerator();
			terrainGenerator.SetConfig(config);			
			terrainGenerator.InitWorldFromConfig(config);

			while (!terrainGenerator.IsInitialized)
				yield return null;
			
			terrainGenerator.GenerateWorld();

			while (!terrainGenerator.IsGenerated)
				yield return null;

			int width = TERRAIN_NUM_COLS;
			int height = TERRAIN_NUM_ROWS;

			Vector3 centerPos = GetWorldPosition();
			Vector3 originPos = centerPos;
			originPos.x -= ( width * 0.5f ) - 0.5f;
			originPos.z -= ( height * 0.5f ) - 0.5f;

			_terrainTiles = new TileMaterial[TERRAIN_NUM_COLS,TERRAIN_NUM_ROWS];
			var tilePrefab = Resources.Load<TileMaterial>("GroundTile");

			for ( int yTile = 0; yTile < height; yTile++ )
			{
				for ( int xTile = 0; xTile < width; xTile++ )
				{
					var worldPos = new Vector3(xTile * TERRAIN_TILE_WIDTH, 0f, yTile * TERRAIN_TILE_HEIGHT);
					var tileInstance = GameObject.Instantiate<TileMaterial>(tilePrefab, worldPos, Quaternion.identity);
					tileInstance.SetTileType(GetTerrainType(terrainGenerator, TERRAIN_NUM_COLS, TERRAIN_NUM_ROWS, xTile, yTile));
					tileInstance.SetTileSize(TERRAIN_TILE_WIDTH, TERRAIN_TILE_HEIGHT);
					_terrainTiles[xTile,yTile] = tileInstance;
				}
			}
		}
		
		private static TileType GetTerrainType(TerrainGenerator terrainGenerator, int cols, int rows, int x, int y)
		{
			WorldTile nTile = terrainGenerator.GetTile(x, y+1);
			WorldTile eTile = terrainGenerator.GetTile(x+1, y);
			WorldTile wTile = terrainGenerator.GetTile(x-1, y);
			WorldTile sTile = terrainGenerator.GetTile(x, y-1);

			if (terrainGenerator.Tiles[x, y].IsOpen())
			{
				// this tile is a road. what is adjacent?
				if (nTile?.IsOpen() == true)
				{
					if (eTile?.IsOpen() == true)
					{
						if (wTile?.IsOpen() == true)
						{
							if (sTile?.IsOpen() == true)
								return TileType.DirtRoad_NEWS;
							else
								return TileType.DirtRoad_NEW;
						}
						else if (sTile?.IsOpen() == true)
							return TileType.DirtRoad_NES;
						else
							return TileType.DirtRoad_NE;
					}
					else if (wTile?.IsOpen() == true)
					{
						if (sTile?.IsOpen() == true)
							return TileType.DirtRoad_NSW;
						else
							return TileType.DirtRoad_NW;
					}
					else if (sTile?.IsOpen() == true)
						return TileType.DirtRoad_NS;
					else
						return TileType.HalfGrass_SN;
				}
				else if (eTile?.IsOpen() == true)
				{
					if (wTile?.IsOpen() == true)
					{
						if (sTile?.IsOpen() == true)
							return TileType.DirtRoad_ESW;
						else
							return TileType.DirtRoad_EW;
					}
					else if (sTile?.IsOpen() == true)
						return TileType.DirtRoad_ES;
					else
						return TileType.HalfGrass_WE;
				}
				else if (wTile?.IsOpen() == true)
				{
					if (sTile?.IsOpen() == true)
						return TileType.DirtRoad_SW;
					else
						return TileType.HalfGrass_EW;
				}
				else if (sTile?.IsOpen() == true)
				{
					return TileType.HalfGrass_NS;
				}
			}
			else
			{
				// this tile is not a road. what is adjacent?
				if (nTile?.IsOpen() == true)
				{
					if (eTile?.IsOpen() == true)
					{
						if (wTile?.IsOpen() == true)
						{
							if (sTile?.IsOpen() == true)
								return TileType.Grass;
							else
								return TileType.Grass2;
						}
						else if (sTile?.IsOpen() == true)
							return TileType.Grass;
						else
							return TileType.Grass2;
					}
					else if (wTile?.IsOpen() == true)
					{
						if (sTile?.IsOpen() == true)
							return TileType.Grass;
						else
							return TileType.Grass2;
					}
					else if (sTile?.IsOpen() == true)
						return TileType.Grass;
					else
						return TileType.HalfGrass_SN;
				}
				else if (eTile?.IsOpen() == true)
				{
					if (wTile?.IsOpen() == true)
					{
						if (sTile?.IsOpen() == true)
							return TileType.Grass;
						else
							return TileType.Grass;
					}
					else if (sTile?.IsOpen() == true)
						return TileType.Grass2;
					else
						return TileType.HalfGrass_WE;
				}
				else if (wTile?.IsOpen() == true)
				{
					if (sTile?.IsOpen() == true)
						return TileType.Grass;
					else
						return TileType.HalfGrass_EW;
				}
				else if (sTile?.IsOpen() == true)
				{
					return TileType.HalfGrass_NS;
				}
			}

			return Random.value > 0.5f ? TileType.Grass : TileType.Grass2;
		}

		private static void EndGame()
		{
			_terrainTiles = null;
			Utilities.LoadScene(SceneName.MainMenu);
		}
	}
}
