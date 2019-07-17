using UnityEngine;

namespace Glazman.Tank
{
	public static class EntityFactory
	{
		public static Entity CreatePlayerTank(string name, Vector3 worldPosition)
		{
			var agent = new AgentModule(name, "AgentController", worldPosition);
			
			var tankModel = new PrefabModule<TankModelBehaviour>("TankModel");
			tankModel.component.SetColor(Color.green);
			tankModel.component.SetAgent(agent);

			var userInput = new UserAgentModule();
			
			var pathfinding = new PathfindingModule();
			
			return new Entity(agent, tankModel, userInput, pathfinding);
		}
		
		public static Entity CreateNpcTank(string name, Vector3 worldPosition)
		{
			var agent = new AgentModule(name, "AgentController", worldPosition);
			
			var tankModel = new PrefabModule<TankModelBehaviour>("TankModel");
			tankModel.component.SetColor(Color.red);
			tankModel.component.SetAgent(agent);
			
			return new Entity(agent, tankModel);
		}

		public static Entity CreateTerrain(string name, Vector3 worldPos, TileType tileType, bool isOpen, float size, int x, int y)
		{
			var transform = new TransformModule(name, worldPos);
			
			var tile = new PrefabModule<TerrainBehaviour>("TerrainTile");
			tile.component.SetTileSize(size, size);
			tile.component.SetTileType(tileType, isOpen);
			tile.component.SetTileCoordinates(x, y);
			
			var terrain = new TerrainModule(x, y, size);
			
			return new Entity(transform, tile, terrain);
		}

		public static Entity CreateProp(string name, string prefabName, Vector3 worldPos)
		{
			var transform = new TransformModule(name, worldPos);
			var prop = new PrefabModule(prefabName);
			
			return new Entity(transform, prop);
		}

		public static Entity CreateDestructible(string name, string prefabName, Vector3 worldPos, int maxHp)
		{
			var transform = new TransformModule(name, worldPos);
			var collision = new CollisionModule();
			var prop = new PrefabModule(prefabName);
			var health = new HealthModule(maxHp, maxHp);
			var destruct = new DestructibleModule();
			
			return new Entity(transform, collision, prop, health, destruct);
		}
		
		public static Entity CreateBullet(string name, string prefabName, Vector3 worldPos, Vector3 velocity)
		{
			var transform = new TransformModule(name, worldPos);
			var collision = new CollisionModule();
			var prop = new PrefabModule(prefabName);
			var bullet = new BulletModule(velocity);
			
			return new Entity(transform, prop, collision, bullet);
		}
	}
}
