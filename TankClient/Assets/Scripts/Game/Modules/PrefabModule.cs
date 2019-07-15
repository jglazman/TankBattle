using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Tank
{
	/// <summary>
	/// Attach an arbitrary prefab to our root Transform.
	/// </summary>
	public class PrefabModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		protected override ModuleType ModuleType { get { return ModuleType.Prefab; } }

		public override ModuleType[] Dependencies { get { return new[] { ModuleType.Transform }; } }


		private GameObject _gameObject;
		public GameObject gameObject => _gameObject;
		
		public PrefabModule(string prefabName)
		{
			var prefab = Resources.Load<GameObject>(prefabName);
			if (prefab == null)
				throw new Exception($"Prefab not found in Resources: {prefabName}");
			
			_gameObject = GameObject.Instantiate(prefab);
		}

		public override void LinkToDependency(Module dependency)
		{
			if (dependency.IsModuleType(ModuleType.Transform))
			{
				// attach to the root transform
				var tm = dependency as TransformModule;
				_gameObject.transform.SetParent(tm.transform, false);
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


	/// <summary>
	/// Attach an arbitrary prefab with a component of type T to our root Transform.
	/// </summary>
	public class PrefabModule<T> : PrefabModule
		where T : UnityEngine.Component
	{
		private T _component;
		public T component => _component;
		
		public PrefabModule(string prefabName)
			: base(prefabName)
		{
			_component = gameObject.GetComponent<T>();

			Assert.IsTrue(_component != null, $"Prefab '{prefabName}' is missing a {typeof(T)} component!");
		}
	}
}
