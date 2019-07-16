using UnityEngine;

namespace Glazman.Tank
{
	/// <summary>
	/// Attach an arbitrary prefab to our root Transform.
	/// </summary>
	public class BulletModule : Module
	{
		public override int Priority { get { return ModulePriority.Bullet; } }

		protected override ModuleType ModuleType { get { return ModuleType.Bullet; } }

		public override ModuleType[] Dependencies { get { return new ModuleType[] { ModuleType.Transform, ModuleType.Collision }; } }

		
		private TransformModule _transform;
		private Vector3 _velocity;
		
		
		public BulletModule(Vector3 velocity)
		{
			_velocity = velocity;
		}

		public override void LinkToDependency(Module dependency)
		{
			if (dependency.IsModuleType(ModuleType.Transform))
			{
				_transform = dependency as TransformModule; 
			}
			
			if (dependency.IsModuleType(ModuleType.Collision))
			{
				(dependency as CollisionModule).collision.SetTriggerCallback(OnTrigger);
			}

		}

		public override void Update(float deltaTime)
		{
			_transform.transform.position += _velocity * deltaTime;
		}

		protected override void DestroyInternal()
		{
			_transform = null;
		}


		private void OnTrigger(CollisionTag tag, Entity otherEntity)
		{
			// we are destroyed if we hit anything
			// TODO: VFX?
			this.entity.Destroy();
		}
	}

}
