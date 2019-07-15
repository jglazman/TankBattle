using System;
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
		// the size of terrain tiles in world units
		private const int TERRAIN_TILE_SIZE = 2;
		
		private static GameState _gameState;
		private static Entity _playerEntity;
		private static List<Entity> _enemyEntities = new List<Entity>();
		private static List<Entity> _terrainEntities = new List<Entity>();
		private static List<Entity> _propEntities = new List<Entity>();


		private static UserInput _userInput;
		
		public static void Initialize()
		{
//			Assert.raiseExceptions = true;
			
			GameUI.ListenForMessages(HandleUIMessage);
			
			_userInput = new UserInput();
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

		private static void StartNewGame(Difficulty difficulty)
		{
			Debug.LogWarning($"Start new game!  Difficulty={difficulty}");

			_gameState = GameState.GameInProgress;
			Utilities.LoadScene(SceneName.Game);

			UnityWrapper.Instance.StartCoroutine(InitializeLevel(difficulty));
		}
		
		private static IEnumerator InitializeLevel(Difficulty difficulty)
		{
			int terrainCols, terrainRows, desiredEnemies;

			// generate terrain data
			WorldTileConfig[,] worldSeed;
			switch (difficulty)
			{
				case Difficulty.Easy:
					worldSeed = WorldGenSeed.SEED_MAZE1;
					terrainCols = 7;
					terrainRows = 7;
					desiredEnemies = 3;
					break;
				
				case Difficulty.Normal:	
					worldSeed = WorldGenSeed.SEED_MAZE1;
					terrainCols = 9;
					terrainRows = 9;
					desiredEnemies = 5;
					break;
				
				case Difficulty.Hard:
					worldSeed = WorldGenSeed.SEED_MAZE1;
					terrainCols = 11;
					terrainRows = 11;
					desiredEnemies = 20;
					break;

				default:
					throw new Exception($"Unhandled difficulty level: {difficulty}");
			}
			
			int randomSeed = UnityEngine.Random.Range(0, 9999999);
			var config = new TerrainGenerator.WorldGenConfig(randomSeed, WorldType.Prim, terrainCols, terrainRows, 
				new Vector2(TERRAIN_TILE_SIZE, TERRAIN_TILE_SIZE), worldSeed);
			
			var terrainGenerator = new TerrainGenerator();
			terrainGenerator.InitWorldFromConfig(config);
			while (!terrainGenerator.IsInitialized)
				yield return null;

			terrainGenerator.GenerateWorld();
			while (!terrainGenerator.IsGenerated)
				yield return null;

			// center the camera over the terrain
			Camera.main.transform.position = new Vector3(terrainCols - 1, (terrainCols -1 ) * 4, terrainRows - 1);

			// spawn terrain tiles
			for ( int yTile = 0; yTile < terrainRows; yTile++ )
			{
				for ( int xTile = 0; xTile < terrainCols; xTile++ )
				{
					bool isRoadTile = terrainGenerator.GetTile(xTile, yTile).IsOpen();
					var tileType = GetTerrainType(terrainGenerator, xTile, yTile);
					var worldPos = GetTileWorldPosition(xTile, yTile);
					var terrain = EntityFactory.CreateTerrain($"Terrain_{xTile}_{yTile}", worldPos, tileType, isRoadTile, TERRAIN_TILE_SIZE);
					_terrainEntities.Add(terrain);

					if (isRoadTile)
					{
						// randomly spawn the player on a road tile
						if (_playerEntity == null && UnityEngine.Random.value > 0.75f)
							_playerEntity = EntityFactory.CreatePlayerTank("Player", worldPos);
					}
					else
					{
						// randomly spawn obstacles
						if (UnityEngine.Random.value > 0.5f)
						{
							var prop = EntityFactory.CreateProp($"Prop_Tree_{_propEntities.Count}", worldPos, "PropTree");
							_propEntities.Add(prop);
						}
					}
				}
			}
			
			// lame fallback in case beat the odds and never placer the player
			if (_playerEntity == null)
			{
				for ( int yTile = 0; yTile < terrainRows; yTile++ )
				{
					for (int xTile = 0; xTile < terrainCols; xTile++)
					{
						int index = terrainGenerator.GetLinearIndex(xTile, yTile);
						var terrain = _terrainEntities[index].GetModule<PrefabModule<TerrainBehaviour>>(ModuleType.Prefab);
						if (terrain.component.isOpen)
						{
							_playerEntity = EntityFactory.CreatePlayerTank("Player", GetTileWorldPosition(xTile, yTile));
							break;
						}
					}

					if (_playerEntity != null)
						break;
				}
			}

			// iterate backwards over the terrain to place enemies preferentially far from the player
			for (int yTile = terrainRows - 1; yTile >= 0; yTile--)
			{
				for (int xTile = terrainCols - 1; xTile >= 0; xTile--)
				{
					int index = terrainGenerator.GetLinearIndex(xTile, yTile);
					var terrain = _terrainEntities[index].GetModule<PrefabModule<TerrainBehaviour>>(ModuleType.Prefab);
					if (terrain.component.isOpen && _enemyEntities.Count < desiredEnemies && UnityEngine.Random.value > 0.8f)
					{
						var enemy = EntityFactory.CreateNpcTank($"Enemy_{_enemyEntities.Count}", GetTileWorldPosition(xTile, yTile));
						_enemyEntities.Add(enemy);
					}
				}
			}
		}

		private static Vector3 GetTileWorldPosition(int col, int row)
		{
			return new Vector3(col * TERRAIN_TILE_SIZE, 0f, row * TERRAIN_TILE_SIZE);
		}
		
		/// <summary>Super brute-force road tile sequencer.</summary>
		private static TileType GetTerrainType(TerrainGenerator terrainGenerator, int x, int y)
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

			// this is not a road tile
			return UnityEngine.Random.value > 0.5f ? TileType.Grass : TileType.Grass2;
		}

		private static void EndGame()
		{
			_playerEntity.Destroy();
			_playerEntity = null;
			
			foreach (var enemy in _enemyEntities)
				enemy.Destroy();
			_enemyEntities.Clear();
			
			foreach (var terrain in _terrainEntities)
				terrain.Destroy();
			_terrainEntities.Clear();
			
			foreach (var prop in _propEntities)
				prop.Destroy();
			_propEntities.Clear();
			
			Utilities.LoadScene(SceneName.MainMenu);
		}
	}
}
