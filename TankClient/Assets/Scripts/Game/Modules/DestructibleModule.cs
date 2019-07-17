using System;
using UnityEngine;

namespace Glazman.Tank
{
	public class DestructibleModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		protected override ModuleType ModuleType { get { return ModuleType.Destructible; } }

		public override ModuleType[] Dependencies { get { return new[] { ModuleType.Prefab, ModuleType.Health, ModuleType.Collision }; } }


		private HealthModule _health;
		private PrefabModule _prefab;
		private float _desiredScale;
		private float _scale;
		
		
		public override void LinkToDependency(Module dependency)
		{
			if (dependency.IsModuleType(ModuleType.Prefab))
			{
				_prefab = dependency as PrefabModule;
			}
			
			if (dependency.IsModuleType(ModuleType.Health))
			{
				_health = dependency as HealthModule;
				_health.OnHealthChanged += OnHealthChanged;
				_desiredScale = (float)_health.HitPoints / (float)_health.MaxHitPoints;
				_scale = 1f;
			}
			
			if (dependency.IsModuleType(ModuleType.Collision))
			{
				(dependency as CollisionModule).collision.SetTriggerCallback(OnTrigger);
			}
		}
		
		private void OnTrigger(CollisionTag tag, Entity otherEntity)
		{
			if (tag == CollisionTag.Bullet)
				_health.ChangeHealth(-1);
		}


		private void OnHealthChanged(int hp, int delta)
		{
			_desiredScale = (float)hp / (float)_health.MaxHitPoints;			
		}

		public override void Update(float deltaTime)
		{
			if (Math.Abs(_scale - _desiredScale) > 0.01f)
			{
				_scale = Mathf.Lerp(_scale, _desiredScale, deltaTime * 3f);
				_prefab.gameObject.transform.localScale = Vector3.one * _scale;
			}
			else if (_health.IsDead)
			{
				// destruct!
				this.entity.Destroy();
			}
		}

		protected override void DestroyInternal()
		{
			_health = null;
			_prefab = null;
		}
	}
}
