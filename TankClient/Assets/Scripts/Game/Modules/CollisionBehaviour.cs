using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	// these must match tags defined in your Unity project
	public enum CollisionTag
	{
		Tank,
		Bullet,
		Prop,
		Wall
	}
	
	public class CollisionBehaviour : MonoBehaviour
	{
		private static Dictionary<string,CollisionTag> _collisionTags;
		
		private System.Action<CollisionTag,Entity> _triggerCallback = null;
		
		
		private void Awake()
		{
			// we need a rigidbody to get collision callbacks
			var rb = gameObject.AddComponent<Rigidbody>();
			rb.isKinematic = true;

			if (_collisionTags == null)
			{
				_collisionTags = new Dictionary<string, CollisionTag>();
				
				var tags = (CollisionTag[])System.Enum.GetValues(typeof(CollisionTag));
				foreach (var collisionTag in tags)
					_collisionTags[collisionTag.ToString()] = collisionTag;
			}
		}

		private void OnTriggerEnter(Collider otherCollider)
		{
			foreach (var collisionTag in _collisionTags)
			{
				if (otherCollider.CompareTag(collisionTag.Key))
				{
					var root = Utilities.GetRootGameObject(otherCollider.gameObject);
					var eb = root.GetComponent<EntityBehaviour>();
					if (eb != null)
					{
						_triggerCallback?.Invoke(collisionTag.Value, eb.Entity);
						break;
					}
				}
			}
		}

		public void SetTriggerCallback(System.Action<CollisionTag,Entity> callback)
		{
			_triggerCallback = callback;
		}	
	}
}
