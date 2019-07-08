using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Glazman.Tank
{
	/// <summary>
	/// A singleton MonoBehaviour to make special Unity methods available via a general interface.
	/// </summary>
	public class UnityWrapper : MonoBehaviour
	{
		private static UnityWrapper _instance = null;
		public static UnityWrapper Instance { get { return _instance; } }

		private readonly List<UnityBehaviour> _registeredBehaviours = new List<UnityBehaviour>();


		public static void Register(UnityBehaviour b)
		{
			Assert.IsTrue(_instance != null, "UnityWrapper instance is null!");
			Assert.IsTrue(!_instance._registeredBehaviours.Contains(b), "Behaviour is already registered with the UnityWrapper!");
			
			_instance._registeredBehaviours.Add(b);
			_instance._registeredBehaviours.Sort((b1, b2) => b1.Priority.CompareTo(b2.Priority));
		}

		public static void Unregister(UnityBehaviour b)
		{
			Assert.IsTrue(_instance != null, "UnityWrapper instance is null!");
			Assert.IsTrue(_instance._registeredBehaviours.Contains(b), "Behaviour is already registered with the UnityWrapper!");

			_instance._registeredBehaviours.Remove(b);
		}


		private void Awake()
		{
			if (_instance != null)
			{
				Debug.LogError("Tried to create another UnityWrapper instance! Destroying the new instance with extreme prejudice...");
				GameObject.Destroy(this.gameObject);
				return;
			}

			_instance = this;
		}
		
		private void OnApplicationFocus(bool focus)
		{
			for (int i = 0; i < _registeredBehaviours.Count; i++)
				_registeredBehaviours[i]?.OnApplicationFocus(focus);
		}
		
		private void Update()
		{
			for (int i = 0; i < _registeredBehaviours.Count; i++)
				_registeredBehaviours[i]?.Update(Time.deltaTime);
		}

		private void FixedUpdate()
		{
			for (int i = 0; i < _registeredBehaviours.Count; i++)
				_registeredBehaviours[i]?.FixedUpdate(Time.fixedDeltaTime);
		}

		private void LateUpdate()
		{
			for (int i = 0; i < _registeredBehaviours.Count; i++)
				_registeredBehaviours[i]?.LateUpdate(Time.deltaTime);
		}

		private void OnDestroy()
		{
			for (int i = 0; i < _registeredBehaviours.Count; i++)
				_registeredBehaviours[i]?.OnDestroy();
			
			_registeredBehaviours.Clear();
		}
	}
}
