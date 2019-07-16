using UnityEngine;

namespace Glazman.Tank
{
	/// <summary>
	/// Create a Transform so we can exist in a Unity scene.
	/// </summary>
	public class TransformModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		protected override ModuleType ModuleType { get { return ModuleType.Transform; } }

		public override ModuleType[] Dependencies { get { return null; } }


		/// <summary>
		/// Derived types are responsible for assigning their own references.
		/// </summary>
		protected TransformModule() { }

		/// <summary>
		/// This will instantiate a new GameObject into the scene.
		/// </summary>
		public TransformModule(string gameObjectName, Vector3 worldPosition)
		{
			_gameObject = new GameObject(gameObjectName);
			_transform = _gameObject.transform;
			_transform.position = worldPosition;
		}


		// our cached transform
		protected Transform _transform;
		public Transform transform => _transform;

		protected GameObject _gameObject;
		public GameObject gameObject => _gameObject;

		protected override void InitializeInternal()
		{
			var eb = _gameObject.AddComponent<EntityBehaviour>();
			eb.SetEntity(this.entity);
		}

		protected override void DestroyInternal()
		{
			if (_gameObject != null)
				GameObject.Destroy(_gameObject);

			_gameObject = null;
			_transform = null;
		}
	}
}
