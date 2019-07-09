using System;
using UnityEngine;

namespace Glazman.Tank
{
	public sealed class PrefabModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		public override ModuleType ModuleType { get { return ModuleType.Prefab; } }

		public override ModuleType[] Dependencies { get { return new ModuleType[] { ModuleType.Transform }; } }


		private GameObject _gameObject;
		

		public PrefabModule(string prefabName)
		{
			var prefab = Resources.Load<GameObject>(prefabName);
			if (prefab == null)
				throw new Exception($"Prefab not found in Resources: {prefabName}");
			
			_gameObject = GameObject.Instantiate(prefab);
		}

		public override void LinkToDependency(Module dependency)
		{
			switch (dependency.ModuleType)
			{
				case ModuleType.Transform:
					_gameObject.transform.SetParent((dependency as TransformModule)?.Transform, false);
					break;
			}
		}
	}
}
