using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	public static class EntityFactory
	{
		public static Entity CreatePlayerTank(string name, Vector3 worldPosition)
		{
			var entity = new Entity();
			entity.AddModule(new AgentModule("AgentController", worldPosition));
			entity.AddModule(new TransformModule(name, worldPosition));
			entity.AddModule(new PrefabModule("TankModel", Color.green));
			return entity;
		}
		
		public static Entity CreateNpcTank(string name, Vector3 worldPosition)
		{
			var entity = new Entity();
			entity.AddModule(new AgentModule("AgentController", worldPosition));
			entity.AddModule(new TransformModule(name, worldPosition));
			entity.AddModule(new PrefabModule("TankModel", Color.red));
			return entity;
		}
	}
}
