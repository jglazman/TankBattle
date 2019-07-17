using UnityEngine;

namespace Glazman.Tank
{
	public static class EntityFactory
	{
		public static Entity CreatePlayerTank(string name, Vector3 worldPosition, int maxHp)
		{
			var agent = new AgentModule(name, "AgentController", worldPosition);
			var userInput = new UserAgentModule();
			var health = new HealthModule(maxHp, maxHp);
			var pathfinding = new PathfindingModule(true);	// only used for debugging
			
			var tankModel = new PrefabModule<TankModelBehaviour>("TankModel");
			tankModel.component.SetColor(Color.green);
			tankModel.component.SetAgent(agent);
			
			return new Entity(health, agent, tankModel, userInput, pathfinding);
		}
		
		public static Entity CreateNpcTank(string name, Vector3 worldPosition, int maxHp)
		{
			var agent = new AgentModule(name, "AgentController", worldPosition);
			var npc = new NpcAgentModule();
			var collision = new CollisionModule();
			var health = new HealthModule(maxHp, maxHp);
			var pathfinding = new PathfindingModule();
			
			var tankModel = new PrefabModule<TankModelBehaviour>("TankModel");
			tankModel.component.SetColor(Color.red);
			tankModel.component.SetAgent(agent);
			
			return new Entity(health, agent, tankModel, collision, pathfinding, npc);
		}

		public static Entity CreateTerrain(string name, Vector3 worldPos, TileType tileType, bool isOpen, float size, int x, int y)
		{
			var transform = new TransformModule(name, worldPos);
			var terrain = new TerrainModule(x, y, size);
			
			var tile = new PrefabModule<TerrainBehaviour>("TerrainTile");
			tile.component.SetTileSize(size, size);
			tile.component.SetTileType(tileType, isOpen);
			tile.component.SetTileCoordinates(x, y);
			
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
