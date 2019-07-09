using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	/// <summary>
	/// Create a Transform so we can exist in a Unity scene.
	/// </summary>
	public sealed class TransformModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		public override ModuleType ModuleType { get { return ModuleType.Transform; } }

		public override ModuleType[] Dependencies { get { return null; } }


		/// <summary>
		/// If you already have a Transform you can set it here.
		/// </summary>
		/// <param name="gameObject"></param>
		public TransformModule(GameObject gameObject)
		{
			if (gameObject == null)
				throw new Exception("Tried to initialize a TransformModule with a null GameObject!");
			
			_transform = gameObject.transform;
		}
		
		/// <summary>
		/// If you want to generate a Transform then request it here.
		/// </summary>
		public TransformModule(string gameObjectName, Vector3 worldPosition)
		{
			var go = new GameObject(gameObjectName);
			_transform = go.transform;
			_transform.position = worldPosition;
		}

		private Transform _transform;
		public Transform Transform { get { return _transform; } }

		public void AttachToParent(Transform parent, Vector3 offset)
		{
			_transform.SetParent(parent, false);
			_transform.localPosition = offset;
		}
	}
}
