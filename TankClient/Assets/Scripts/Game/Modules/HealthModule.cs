using System;
using UnityEngine;

namespace Glazman.Tank
{
	public class HealthModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		protected override ModuleType ModuleType { get { return ModuleType.Health; } }

		public override ModuleType[] Dependencies { get { return null; } }


		private int _maxHitPoints;
		public int MaxHitPoints => _maxHitPoints;
		
		private int _hitPoints;
		public int HitPoints => _hitPoints;

		public bool IsDead => _hitPoints <= 0;
		

		public HealthModule(int maxHitPoints, int hitPoints)
		{
			_maxHitPoints = maxHitPoints;
			_hitPoints = hitPoints;
		}

		public void ChangeHealth(int delta)
		{
			if (_hitPoints <= 0 && delta < 0)
				return;	// already dead
			
			var newHp = Mathf.Clamp(_hitPoints + delta, 0, _maxHitPoints);
			bool didChange = newHp != _hitPoints;
			_hitPoints = newHp;
			
			if (didChange)
				OnHealthChanged?.Invoke(_hitPoints, delta);
		}


		public delegate void HealthChangedDelegate(int hp, int delta);

		public event HealthChangedDelegate OnHealthChanged;
	}
}
