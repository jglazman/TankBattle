using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Tank
{
	public static class GameConfig
	{
		// the size of terrain tiles in world units
		public const float TERRAIN_TILE_SIZE = 1f;

		// the speed of a tank
		public const float TANK_SPEED = 2f;
		
		// the speed of projectiles
		public const float BULLET_SPEED = 3f;
	}
	
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
		private static Entity _playerEntity;
		private static TerrainGenerator _terrainGenerator;
		private static List<Entity> _enemyEntities = new List<Entity>();
		private static List<Entity> _terrainEntities = new List<Entity>();
		private static List<Entity> _propEntities = new List<Entity>();
		private static List<Entity> _wallEntities = new List<Entity>();

		// TODO: this is a lame way of exposing the current terrain to pathfinding
		public static TerrainGenerator TerrainGen => _terrainGenerator;
		public static List<Entity> TerrainEntities => _terrainEntities;

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
			int worldSize, desiredEnemies;
			float tileSize = GameConfig.TERRAIN_TILE_SIZE;

			// generate terrain data
			WorldTileConfig[,] worldSeed;
			switch (difficulty)
			{
				case Difficulty.Easy:
					worldSeed = WorldGenSeed.SEED_MAZE1;
					worldSize = 7;
					desiredEnemies = 3;
					break;
				
				case Difficulty.Normal:	
					worldSeed = WorldGenSeed.SEED_MAZE1;
					worldSize = 9;
					desiredEnemies = 5;
					break;
				
				case Difficulty.Hard:
					worldSeed = WorldGenSeed.SEED_MAZE1;
					worldSize = 11;
					desiredEnemies = 20;
					break;

				default:
					throw new Exception($"Unhandled difficulty level: {difficulty}");
			}
			
			int randomSeed = UnityEngine.Random.Range(0, 9999999);
			var config = new TerrainGenerator.WorldGenConfig(randomSeed, WorldType.Prim, worldSize, worldSize, 
				new Vector2(tileSize, tileSize), worldSeed);
			
			_terrainGenerator = new TerrainGenerator();
			_terrainGenerator.InitWorldFromConfig(config);
			while (!_terrainGenerator.IsInitialized)
				yield return null;

			_terrainGenerator.GenerateWorld();
			while (!_terrainGenerator.IsGenerated)
				yield return null;

			// center the camera over the terrain
			Camera.main.transform.position = new Vector3(
				(worldSize - 1) * (tileSize * 0.5f), 
				(worldSize - 1) * (tileSize * 2f), 
				(worldSize - 1) * (tileSize * 0.5f)
			);

			// spawn terrain tiles
			for ( int yTile = 0; yTile < worldSize; yTile++ )
			{
				for ( int xTile = 0; xTile < worldSize; xTile++ )
				{
					bool isRoadTile = _terrainGenerator.GetTile(xTile, yTile).IsOpen();
					var tileType = GetTerrainType(_terrainGenerator, xTile, yTile);
					var worldPos = GetTileWorldPosition(xTile, yTile);
					var terrain = EntityFactory.CreateTerrain($"Terrain_{xTile}_{yTile}", worldPos, tileType, isRoadTile, tileSize, xTile, yTile);
					_terrainEntities.Add(terrain);

					if (!isRoadTile)
					{
						// randomly spawn obstacles
						if (UnityEngine.Random.value > 0.5f)
						{
							var prop = EntityFactory.CreateProp($"Prop_Tree_{_propEntities.Count}", "PropTree", worldPos);
							_propEntities.Add(prop);
						}
					}
				}
			}
			
			// surround the world with invisible walls
			for (int yTile = -1; yTile <= worldSize; yTile++)
			{
				var wall1 = EntityFactory.CreateProp("Blocker", "PropInvisibleWall", GetTileWorldPosition(-1, yTile));
				_wallEntities.Add(wall1);
				
				var wall2 = EntityFactory.CreateProp("Blocker", "PropInvisibleWall", GetTileWorldPosition(worldSize, yTile));
				_wallEntities.Add(wall2);
			}
			
			for (int xTile = -1; xTile <= worldSize; xTile++)
			{
				var wall1 = EntityFactory.CreateProp("Blocker", "PropInvisibleWall", GetTileWorldPosition(xTile, -1));
				_wallEntities.Add(wall1);
				
				var wall2 = EntityFactory.CreateProp("Blocker", "PropInvisibleWall", GetTileWorldPosition(xTile, worldSize));
				_wallEntities.Add(wall2);
			}

			// place the player on a road
			int xPlayer = -1;
			int yPlayer = -1;

			for ( int yTile = 0; yTile < worldSize; yTile++ )
			{
				for (int xTile = 0; xTile < worldSize; xTile++)
				{
					int index = _terrainGenerator.GetLinearIndex(xTile, yTile);
					var terrain = _terrainEntities[index].GetModule<PrefabModule<TerrainBehaviour>>(ModuleType.Prefab);
					if (terrain.component.isOpen)
					{
						_playerEntity = EntityFactory.CreatePlayerTank("Player", GetTileWorldPosition(xTile, yTile));
						xPlayer = xTile;
						yPlayer = yTile;
						break;
					}
				}

				if (_playerEntity != null)
					break;
			}

			// iterate backwards over the terrain to place enemies preferentially far from the player
			for (int yTile = worldSize - 1; yTile >= 0; yTile--)
			{
				for (int xTile = worldSize - 1; xTile >= 0; xTile--)
				{
					int index = _terrainGenerator.GetLinearIndex(xTile, yTile);
					var terrain = _terrainEntities[index].GetModule<PrefabModule<TerrainBehaviour>>(ModuleType.Prefab);
					
					if (terrain.component.isOpen &&
					    !(xTile == xPlayer && yTile == yPlayer) &&
					    _enemyEntities.Count < desiredEnemies &&
					    UnityEngine.Random.value > 0.8f)
					{
						var enemy = EntityFactory.CreateNpcTank($"Enemy_{_enemyEntities.Count}", GetTileWorldPosition(xTile, yTile));
						_enemyEntities.Add(enemy);
					}
				}
			}
		}

		public static Vector3 GetTileWorldPosition(int col, int row)
		{
			return new Vector3(col * GameConfig.TERRAIN_TILE_SIZE, 0f, row * GameConfig.TERRAIN_TILE_SIZE);
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
			_terrainGenerator = null;
			
			_playerEntity?.Destroy();
			_playerEntity = null;
			
			foreach (var enemy in _enemyEntities)
				enemy?.Destroy();
			_enemyEntities.Clear();
			
			foreach (var terrain in _terrainEntities)
				terrain?.Destroy();
			_terrainEntities.Clear();
			
			foreach (var prop in _propEntities)
				prop?.Destroy();
			_propEntities.Clear();
			
			foreach (var wall in _wallEntities)
				wall?.Destroy();
			_wallEntities.Clear();
			
			_gameState = GameState.MainMenu;
			Utilities.LoadScene(SceneName.MainMenu);
		}
	}
}
