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
			
			return new Entity(agent, tankModel, userInput);
		}
		
		public static Entity CreateNpcTank(string name, Vector3 worldPosition)
		{
			var agent = new AgentModule(name, "AgentController", worldPosition);
			
			var tankModel = new PrefabModule<TankModelBehaviour>("TankModel");
			tankModel.component.SetColor(Color.red);
			tankModel.component.SetAgent(agent);
			
			return new Entity(agent, tankModel);
		}

		public static Entity CreateTerrain(string name, Vector3 worldPos, TileType tileType, bool isOpen, float size)
		{
			var transform = new TransformModule(name, worldPos);
			
			var terrain = new PrefabModule<TerrainBehaviour>("TerrainTile");
			terrain.component.SetTileSize(size, size);
			terrain.component.SetTileType(tileType, isOpen);
			
			return new Entity(transform, terrain);
		}

		public static Entity CreateProp(string name, Vector3 worldPos, string prefabName)
		{
			var transform = new TransformModule(name, worldPos);
			var prop = new PrefabModule(prefabName);
			
			return new Entity(transform, prop);
		}
	}
}
