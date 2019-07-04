using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Tank
{
	public class Bootstrap : MonoBehaviour
	{
		private void Awake()
		{
			Game.Initialize();
		}
		
	}
}
