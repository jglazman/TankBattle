using System;
using UnityEngine;

namespace Glazman.Tank
{
	public class CollisionModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		protected override ModuleType ModuleType { get { return ModuleType.Collision; } }

		public override ModuleType[] Dependencies { get { return new[] { ModuleType.Transform }; } }


		private CollisionBehaviour _collision;
		public CollisionBehaviour collision => _collision;
		
		public override void LinkToDependency(Module dependency)
		{
			if (dependency.IsModuleType(ModuleType.Transform))
			{
				_collision = (dependency as TransformModule).gameObject.AddComponent<CollisionBehaviour>();
			}
		}
		
		protected override void DestroyInternal()
		{
			_collision = null;
		}
	}
}
