using UnityEngine;

namespace Glazman.Tank
{
	public static class EntityFactory
	{
		public static Entity CreatePlayerTank(string name, Vector3 worldPosition)
		{
			var agent = new AgentModule(name, "AgentController", worldPosition);
			var tankModel = new PrefabModule<ColorizableBehaviour>("TankModel");
			tankModel.component.SetColor(Color.green);
			
			return new Entity(agent, tankModel);
		}
		
		public static Entity CreateNpcTank(string name, Vector3 worldPosition)
		{
			var agent = new AgentModule(name, "AgentController", worldPosition);
			var tankModel = new PrefabModule<ColorizableBehaviour>("TankModel");
			tankModel.component.SetColor(Color.red);
			
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
	}
}
