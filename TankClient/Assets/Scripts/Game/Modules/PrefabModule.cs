using System;
using System.Net;
using UnityEngine;

namespace Glazman.Tank
{
	/// <summary>
	/// Attach an arbitrary prefab to our root Transform.
	/// </summary>
	public sealed class PrefabModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		public override ModuleType ModuleType { get { return ModuleType.Prefab; } }

		public override ModuleType[] Dependencies { get { return new ModuleType[] { ModuleType.Transform }; } }


		private GameObject _gameObject;
		public GameObject GameObject { get { return _gameObject; } }

		
		public PrefabModule(string prefabName, Color color)
		{
			var prefab = Resources.Load<GameObject>(prefabName);
			if (prefab == null)
				throw new Exception($"Prefab not found in Resources: {prefabName}");
			
			_gameObject = GameObject.Instantiate(prefab);

			// @todo: we could give this behaviour its own module, but time is money
			var colorize = _gameObject.GetComponent<ColorizableBehaviour>();
			if (colorize != null)
				colorize.SetColor(color);
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
		
		
		protected override void DestroyInternal()
		{
			if (_gameObject != null)
			{
				GameObject.Destroy(_gameObject);
				_gameObject = null;
			}
			}
		}
	}
}
