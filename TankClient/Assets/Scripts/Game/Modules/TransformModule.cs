using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	public sealed class TransformModule : Module
	{
		public override int Priority { get { return ModulePriority.Default; } }

		public override ModuleType ModuleType { get { return ModuleType.Transform; } }

		public override ModuleType[] Dependencies { get { return null; } }


		public TransformModule(string gameObjectName, Vector3 worldPosition)
		{
			var go = new GameObject(gameObjectName);
			this.Transform = go.transform;
			this.Transform.position = worldPosition;
		}

		public Transform Transform { get; private set; }
	}
}
