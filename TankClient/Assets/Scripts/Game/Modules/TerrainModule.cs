using System;
using UnityEngine;

namespace Glazman.Tank
{
	public class TerrainModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		protected override ModuleType ModuleType { get { return ModuleType.Terrain; } }

		public override ModuleType[] Dependencies { get { return new[] { ModuleType.Transform }; } }


		private TransformModule _transform;
		//private int _obstacleMask = LayerMask.GetMask("Tank", "Prop");
		private int _obstacleMask = LayerMask.GetMask("Prop");
		private float _radius;
		private int _x;
		private int _y;

		public int X => _x;
		public int Y => _y;


		public Transform transform => _transform?.transform;
		
		public TerrainModule(int x, int y, float size)
		{
			_x = x;
			_y = y;
			_radius = size * 0.3f;
		}

		public override void LinkToDependency(Module dependency)
		{
			if (dependency.IsModuleType(ModuleType.Transform))
			{
				_transform = dependency as TransformModule;
				_ray = new Ray(_transform.transform.position, Vector3.up);
			}
		}
		
		protected override void DestroyInternal()
		{
			_transform = null;
		}

		private Ray _ray;

		public bool IsOpen()
		{
			// check if anything is sitting on top of us
			if (Physics.SphereCastAll(_ray, _radius, _radius, _obstacleMask).Length > 0)
			{
				return false;
			}

			return true;
		}
	}

}
