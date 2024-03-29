﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	/// <summary>
	/// We could procedurally generate everything at runtime, but sometimes this is faster and easier.
	/// </summary>
	public class AgentBehaviour : MonoBehaviour
	{
		[SerializeField] private CharacterController _controller;

		public CharacterController controller => _controller;
	}
}
