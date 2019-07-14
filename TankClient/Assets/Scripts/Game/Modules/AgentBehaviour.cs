using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	/// <summary>
	/// We could procedurally generate everything at runtime, but sometimes this is faster and easier.
	/// </summary>
	public class AgentBehaviour : MonoBehaviour
	{
		[SerializeField] private Collider _triggerCollider;
		[SerializeField] private CharacterController _controller;

		public Collider trigger => _triggerCollider;
		public CharacterController controller => _controller;
	}
}
