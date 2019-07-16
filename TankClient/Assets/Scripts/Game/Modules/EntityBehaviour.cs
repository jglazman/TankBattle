using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	/// <summary>
	/// Link a GameObject to an Entity.
	/// </summary>
	public class EntityBehaviour : MonoBehaviour
	{
		private Entity _entity = null;
		public Entity Entity => _entity;
		
		public void SetEntity(Entity e)
		{
			_entity = e;
		}
	}
}
